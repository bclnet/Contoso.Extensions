using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.Distributed
{
    /// <summary>
    /// Manages the DistributedCache.
    /// </summary>
    public static class DistributedCacheManager
    {
        static readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        readonly static byte[] EmptyValue = new byte[] { 0 };

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        public static void Remove(this IDistributedCache cache, DistributedCacheRegistration key, params object[] values) => cache.Remove(key.GetName(values));
        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified key]; otherwise, <c>false</c>.
        /// </returns>
        public static bool Contains(this IDistributedCache cache, DistributedCacheRegistration key, params object[] values) => cache.Contains(key.GetName(values));

        /// <summary>
        /// Touches the specified names.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="names">The names.</param>
        public static void Touch(this IDistributedCache cache, string[] names)
        {
            if (names == null || names.Length == 0)
                return;
            foreach (var name in names)
            {
                if (name.StartsWith("#"))
                {
                    FileCacheDependency.CacheFile.Touch(name);
                    continue;
                }
                cache.Set(name, EmptyValue);
            }
        }

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">key</exception>
        public static T Get<T>(this IDistributedCache cache, DistributedCacheRegistration key, object tag, object[] values) => GetOrCreateUsingLock<T>(cache, key ?? throw new ArgumentNullException(nameof(key)), tag, values);
        /// <summary>
        /// Gets the specified key as DistributedCacheResult.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public static DistributedCacheResult GetResult(this IDistributedCache cache, DistributedCacheRegistration key, object tag, params object[] values) => Get<DistributedCacheResult>(cache, key, tag, values);
        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">key</exception>
        public static async Task<T> GetAsync<T>(this IDistributedCache cache, DistributedCacheRegistration key, object tag, object[] values) => await GetOrCreateUsingLockAsync<T>(cache, key ?? throw new ArgumentNullException(nameof(key)), tag, values);
        /// <summary>
        /// Gets the specified key as DistributedCacheResult.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public static async Task<DistributedCacheResult> GetResultAsync(this IDistributedCache cache, DistributedCacheRegistration key, object tag, params object[] values) => await GetAsync<DistributedCacheResult>(cache, key, tag, values);

        /// <summary>
        /// Adds the specified name.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="entryOptions">The entry options.</param>
        /// <exception cref="InvalidOperationException">Not Service Cache Result</exception>
        public static void Add(this IDistributedCache cache, string name, object value, Action<DistributedCacheEntryOptions> entryOptions)
        {
            _rwLock.EnterWriteLock();
            try
            {
                if (value is DistributedCacheResult valueResult)
                {
                    entryOptions?.Invoke(valueResult.EntryOptions);
                    SetValueWithETagInsideLock(cache, name, valueResult);
                }
                else throw new InvalidOperationException("Not Service Cache Result");
            }
            finally { _rwLock.ExitWriteLock(); }
        }

        static IEnumerable<IChangeToken> MakeChangeTokens(IDistributedCache cache, object tag, IEnumerable<string> names) =>
            names.Select(x =>
            {
                if (x.StartsWith("#")) return new { name = x.Substring(1), regionName = "#" };
                var name = x;
                // add anchor name if not exists
                if (cache.Get(name) == null) cache.Set(name, EmptyValue);
                return new { name, regionName = string.Empty };
            }).GroupBy(x => x.regionName).Select(x =>
                x.Key == "#" ? FileCacheDependency.CacheFile.MakeFileWatchChangeToken(x.Select(y => y.name))
                : cache.MakeCacheEntryChangeToken(x.Select(y => y.name))
            ).Where(x => x != null).ToList();

        static T GetOrCreateUsingLock<T>(IDistributedCache cache, DistributedCacheRegistration key, object tag, object[] values)
        {
            var rwLock = key._rwLock ?? _rwLock;
            var name = key.GetName(values);
            rwLock.EnterUpgradeableReadLock();
            var notCacheResult = typeof(T) != typeof(DistributedCacheResult);
            try
            {
                // double lock test
                var value = Deserialize(cache.Get(name));
                if (value != null)
                    return notCacheResult && value is DistributedCacheResult cacheResult ? (T)cacheResult.Result : (T)value;
                rwLock.EnterWriteLock();
                try
                {
                    value = Deserialize(cache.Get(name));
                    if (value != null)
                        return notCacheResult && value is DistributedCacheResult cacheResult ? (T)cacheResult.Result : (T)value;
                    // create value
                    value = key.BuilderAsync != null ? Task.Run(() => CreateValueAsync<T>(key, tag, values)).GetAwaiter().GetResult() : CreateValue<T>(key, tag, values);
                    if (value == DistributedCacheResult.CacheResult)
                    {
                        value = Deserialize(cache.Get(name));
                        return value != null
                            ? notCacheResult && value is DistributedCacheResult cacheResult ? (T)cacheResult.Result : (T)value
                            : default;
                    }
                    else if (value == DistributedCacheResult.NoResult)
                        return default;
                    // cache value
                    var entryOptions = key.EntryOptions is DistributedCacheEntryOptions2 entryOptions2 ? entryOptions2.ToEntryOptions() : key.EntryOptions;
                    if (key.CacheTags != null)
                    {
                        var tags = key.CacheTags(tag, values);
                        if (tags != null && tags.Any() && entryOptions is DistributedCacheEntryOptions2 entryOptions3)
                            ((List<IChangeToken>)entryOptions3.ExpirationTokens).AddRange(MakeChangeTokens(cache, tag, tags));
                    }
                    var valueAsResult = value is DistributedCacheResult result ? result : new DistributedCacheResult(value);
                    valueAsResult.WeakTag = new WeakReference(tag);
                    valueAsResult.Key = key;
                    valueAsResult.EntryOptions = entryOptions;
                    SetValueWithETagInsideLock(cache, name, valueAsResult);
                    return (T)value;
                }
                catch (Exception e) { throw e; }
                finally { rwLock.ExitWriteLock(); }
            }
            finally { rwLock.ExitUpgradeableReadLock(); }
        }

        static Task<T> GetOrCreateUsingLockAsync<T>(IDistributedCache cache, DistributedCacheRegistration key, object tag, object[] values)
        {
            var rwLock = key._rwLock ?? _rwLock;
            var name = key.GetName(values);
            rwLock.EnterUpgradeableReadLock();
            var notCacheResult = typeof(T) != typeof(DistributedCacheResult);
            try
            {
                // double lock test
                var value = Deserialize(cache.Get(name));
                if (value != null)
                    return Task.FromResult(notCacheResult && value is DistributedCacheResult cacheResult ? (T)cacheResult.Result : (T)value);
                rwLock.EnterWriteLock();
                try
                {
                    value = Deserialize(cache.Get(name));
                    if (value != null)
                        return Task.FromResult(notCacheResult && value is DistributedCacheResult cacheResult ? (T)cacheResult.Result : (T)value);
                    // create value
                    value = key.BuilderAsync != null ? Task.Run(() => CreateValueAsync<T>(key, tag, values)).Result : CreateValue<T>(key, tag, values);
                    if (value == DistributedCacheResult.CacheResult)
                    {
                        value = Deserialize(cache.Get(name));
                        return value != null
                            ? Task.FromResult(notCacheResult && value is DistributedCacheResult cacheResult ? (T)cacheResult.Result : (T)value)
                            : default;
                    }
                    if (value == DistributedCacheResult.NoResult)
                        return Task.FromResult(default(T));
                    // cache value
                    var entryOptions = key.EntryOptions is DistributedCacheEntryOptions2 entryOptions2 ? entryOptions2.ToEntryOptions() : key.EntryOptions;
                    if (key.CacheTags != null)
                    {
                        var tags = key.CacheTags(tag, values);
                        if (tags != null && tags.Any() && entryOptions is DistributedCacheEntryOptions2 entryOptions3)
                            ((List<IChangeToken>)entryOptions3.ExpirationTokens).AddRange(MakeChangeTokens(cache, tag, tags));
                    }
                    var valueAsResult = value is DistributedCacheResult result ? result : new DistributedCacheResult(value);
                    valueAsResult.WeakTag = new WeakReference(tag);
                    valueAsResult.Key = key;
                    valueAsResult.EntryOptions = entryOptions;
                    SetValueWithETagInsideLock(cache, name, valueAsResult);
                    return Task.FromResult((T)value);
                }
                catch (Exception e) { throw e; }
                finally { rwLock.ExitWriteLock(); }
            }
            finally { rwLock.ExitUpgradeableReadLock(); }
        }

        static void SetValueWithETagInsideLock(IDistributedCache cache, string name, DistributedCacheResult value)
        {
            try { cache.Set(name, Serialize(value), value.EntryOptions); }
            catch (InvalidOperationException) { }
            catch (Exception e) { Console.WriteLine(e); }
            finally
            {
                if (!string.IsNullOrEmpty(value.ETag))
                {
                    var etagName = value.Key.GetName(new[] { value.ETag });
                    // ensure base is still exists, then add
                    var baseValue = cache.Get(name);
                    if (baseValue != null)
                        cache.Set(etagName, Encoding.UTF8.GetBytes(name), new DistributedCacheEntryOptions2().SetCacheEntryChangeExpiration(cache, name));
                }
            }
        }

        static T CreateValue<T>(DistributedCacheRegistration key, object tag, object[] values) => (T)key.Builder(tag, values);
        static async Task<T> CreateValueAsync<T>(DistributedCacheRegistration key, object tag, object[] values) => (T)await key.BuilderAsync(tag, values);

        /// <summary>
        /// The serializer.
        /// </summary>
        public static Func<object, byte[]> Serialize = (value) =>
        {
            if (value == null)
                return null;
            using (var s = new MemoryStream())
            using (var w = new BinaryWriter(s))
            {
                var type = value.GetType();
                w.Write(type.AssemblyQualifiedName);
                w.Write(JsonSerializer.Serialize(value, type));
                return Compress(s.ToArray());
            }
        };
        /// <summary>
        /// The deserializer.
        /// </summary>
        public static Func<byte[], object> Deserialize = (value) =>
        {
            if (value == null)
                return null;
            using (var s = new MemoryStream(Decompress(value)))
            using (var r = new BinaryReader(s))
            {
                var type = Type.GetType(r.ReadString(), true);
                return JsonSerializer.Deserialize(r.ReadString(), type);
            }
        };

        static byte[] Compress(byte[] input)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var zip = new GZipStream(outputStream, CompressionMode.Compress))
                    zip.Write(input, 0, input.Length);
                return outputStream.ToArray();
            }
        }

        static byte[] Decompress(byte[] input)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var inputStream = new MemoryStream(input))
                using (var zip = new GZipStream(inputStream, CompressionMode.Decompress))
                    zip.CopyTo(outputStream);
                return outputStream.ToArray();
            }
        }
    }
}
