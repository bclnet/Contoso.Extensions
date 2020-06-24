using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.Memory
{
    public static class MemoryCacheManager
    {
        static readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        readonly static object EmptyValue = new object();

        public static void Remove(this IMemoryCache cache, MemoryCacheRegistration key, params object[] values) => cache.Remove(key.GetName(values));
        public static bool Contains(this IMemoryCache cache, MemoryCacheRegistration key, params object[] values) => cache.Contains(key.GetName(values));

        public static void Touch(this IMemoryCache cache, params string[] names)
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
                //cache.Remove(name);
                cache.Set(name, EmptyValue);
            }
        }

        public static T Get<T>(this IMemoryCache cache, MemoryCacheRegistration key, object tag, params object[] values) => GetOrCreateUsingLock<T>(cache, key ?? throw new ArgumentNullException(nameof(key)), tag, values);
        public static MemoryCacheResult GetResult(this IMemoryCache cache, MemoryCacheRegistration key, object tag, params object[] values) => Get<MemoryCacheResult>(cache, key, tag, values);
        public static async Task<T> GetAsync<T>(this IMemoryCache cache, MemoryCacheRegistration key, object tag, params object[] values) => await GetOrCreateUsingLockAsync<T>(cache, key ?? throw new ArgumentNullException(nameof(key)), tag, values);
        public static async Task<MemoryCacheResult> GetResultAsync(this IMemoryCache cache, MemoryCacheRegistration key, object tag, params object[] values) => await GetAsync<MemoryCacheResult>(cache, key, tag, values);

        public static void Set(this IMemoryCache cache, string name, object value, Action<MemoryCacheEntryOptions> entryOptions)
        {
            _rwLock.EnterWriteLock();
            try
            {
                if (value is MemoryCacheResult valueResult)
                {
                    entryOptions?.Invoke(valueResult.EntryOptions);
                    SetValueWithETagInsideLock(cache, name, valueResult);
                }
                else throw new InvalidOperationException("Not Service Cache Result");
            }
            finally { _rwLock.ExitWriteLock(); }
        }

        static IEnumerable<IChangeToken> MakeChangeTokens(IMemoryCache cache, object tag, IEnumerable<string> names) =>
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

        static T GetOrCreateUsingLock<T>(IMemoryCache cache, MemoryCacheRegistration key, object tag, object[] values)
        {
            var rwLock = key._rwLock ?? _rwLock;
            var name = key.GetName(values);
            rwLock.EnterUpgradeableReadLock();
            var notCacheResult = typeof(T) != typeof(MemoryCacheResult);
            try
            {
                // double lock test
                var value = cache.Get(name);
                if (value != null)
                    return notCacheResult && value is MemoryCacheResult cacheResult ? (T)cacheResult.Result : (T)value;
                rwLock.EnterWriteLock();
                try
                {
                    value = cache.Get(name);
                    if (value != null)
                        return notCacheResult && value is MemoryCacheResult cacheResult ? (T)cacheResult.Result : (T)value;
                    // create value
                    value = key.BuilderAsync != null ? Task.Run(() => CreateValueAsync<T>(key, tag, values)).Result : CreateValue<T>(key, tag, values);
                    if (value == MemoryCacheResult.CacheResult)
                    {
                        value = cache.Get(name);
                        return value != null
                            ? notCacheResult && value is MemoryCacheResult cacheResult ? (T)cacheResult.Result : (T)value
                            : default;
                    }
                    else if (value == MemoryCacheResult.NoResult)
                        return default;
                    // cache value
                    var entryOptions = key.EntryOptions is MemoryCacheEntryOptions2 entryOptions2 ? entryOptions2.ToEntryOptions() : key.EntryOptions;
                    if (key.CacheTags != null)
                    {
                        var tags = key.CacheTags(tag, values);
                        if (tags != null && tags.Any())
                            ((List<IChangeToken>)entryOptions.ExpirationTokens).AddRange(MakeChangeTokens(cache, tag, tags));
                    }
                    if (key.PostEvictionCallback != null)
                        entryOptions.RegisterPostEvictionCallback(key.PostEvictionCallback, tag);
                    var valueAsResult = value is MemoryCacheResult result ? result : new MemoryCacheResult(value);
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

        static Task<T> GetOrCreateUsingLockAsync<T>(IMemoryCache cache, MemoryCacheRegistration key, object tag, object[] values)
        {
            var rwLock = key._rwLock ?? _rwLock;
            var name = key.GetName(values);
            rwLock.EnterUpgradeableReadLock();
            var notCacheResult = typeof(T) != typeof(MemoryCacheResult);
            try
            {
                // double lock test
                var value = cache.Get(name);
                if (value != null)
                    return Task.FromResult(notCacheResult && value is MemoryCacheResult cacheResult ? (T)cacheResult.Result : (T)value);
                rwLock.EnterWriteLock();
                try
                {
                    value = cache.Get(name);
                    if (value != null)
                        return Task.FromResult(notCacheResult && value is MemoryCacheResult cacheResult ? (T)cacheResult.Result : (T)value);
                    // create value
                    value = key.BuilderAsync != null ? Task.Run(() => CreateValueAsync<T>(key, tag, values)).GetAwaiter().GetResult() : CreateValue<T>(key, tag, values);
                    if (value == MemoryCacheResult.CacheResult)
                    {
                        value = cache.Get(name);
                        return value != null
                            ? Task.FromResult(notCacheResult && value is MemoryCacheResult cacheResult ? (T)cacheResult.Result : (T)value)
                            : default;
                    }
                    if (value == MemoryCacheResult.NoResult)
                        return Task.FromResult(default(T));
                    // cache value
                    var entryOptions = key.EntryOptions is MemoryCacheEntryOptions2 entryOptions2 ? entryOptions2.ToEntryOptions() : key.EntryOptions;
                    if (key.CacheTags != null)
                    {
                        var tags = key.CacheTags(tag, values);
                        if (tags != null && tags.Any())
                            ((List<IChangeToken>)entryOptions.ExpirationTokens).AddRange(MakeChangeTokens(cache, tag, tags));
                    }
                    if (key.PostEvictionCallback != null)
                        entryOptions.RegisterPostEvictionCallback(key.PostEvictionCallback, tag);
                    // add value
                    var valueAsResult = value is MemoryCacheResult result ? result : new MemoryCacheResult(value);
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

        static void SetValueWithETagInsideLock(IMemoryCache cache, string name, MemoryCacheResult value)
        {
            try { cache.Set(name, value, value.EntryOptions); }
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
                        cache.Set(etagName, name, new MemoryCacheEntryOptions().SetCacheEntryChangeExpiration(cache, name));
                }
            }
        }

        static T CreateValue<T>(MemoryCacheRegistration key, object tag, object[] values) => (T)key.Builder(tag, values);
        static async Task<T> CreateValueAsync<T>(MemoryCacheRegistration key, object tag, object[] values) => (T)await key.BuilderAsync(tag, values);
    }
}
