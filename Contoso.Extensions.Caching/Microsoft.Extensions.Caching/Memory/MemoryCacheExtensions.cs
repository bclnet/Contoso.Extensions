using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Microsoft.Extensions.Caching.Memory
{
    /// <summary>
    /// Extensions to IMemoryCache.
    /// </summary>
    public static class MemoryCacheExtensions
    {
        static PropertyInfo EntriesCollectionProperty;

        /// <summary>
        /// Gets the entries collection.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <returns></returns>
        public static System.Collections.IDictionary GetEntriesCollection(this IMemoryCache cache)
        {
            if (EntriesCollectionProperty == null)
                EntriesCollectionProperty = cache.GetType().GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
            return EntriesCollectionProperty.GetValue(cache) as System.Collections.IDictionary;
        }

        /// <summary>
        /// Tries to get the entry.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <param name="entry">The entry.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool TryGetEntry(this IMemoryCache cache, string key, out ICacheEntry entry)
        {
            if (EntriesCollectionProperty == null)
                EntriesCollectionProperty = cache.GetType().GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
            var dictionary = EntriesCollectionProperty.GetValue(cache) as System.Collections.IDictionary;
            entry = (ICacheEntry)dictionary[key];
            return entry != null;
        }

        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if [contains] [the specified key]; otherwise, <c>false</c>.</returns>
        public static bool Contains(this IMemoryCache cache, string key) => cache.TryGetValue(key, out _);

        /// <summary>
        /// Sets the cache entry change expiration.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <returns>MemoryCacheEntryOptions.</returns>
        public static MemoryCacheEntryOptions SetCacheEntryChangeExpiration(this MemoryCacheEntryOptions options, IMemoryCache cache, string key)
        {
            options.AddExpirationToken(MakeCacheEntryChangeToken(cache, new[] { key }));
            return options;
        }

        /// <summary>
        /// Sets the file watch expiration.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>MemoryCacheEntryOptions.</returns>
        public static MemoryCacheEntryOptions SetFileWatchExpiration(this MemoryCacheEntryOptions options, string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            var fileProvider = new PhysicalFileProvider(fileInfo.DirectoryName);
            options.AddExpirationToken(fileProvider.Watch(fileInfo.Name));
            return options;
        }

        /// <summary>
        /// Makes the cache entry change token.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="keys">The keys.</param>
        /// <returns>IChangeToken.</returns>
        public static IChangeToken MakeCacheEntryChangeToken(this IMemoryCache cache, IEnumerable<string> keys)
        {
            if (EntriesCollectionProperty == null)
                EntriesCollectionProperty = cache.GetType().GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
            var dictionary = EntriesCollectionProperty.GetValue(cache) as System.Collections.IDictionary;
            var token = new CacheEntryChangeToken();
            foreach (var entry in keys.Select(key => (ICacheEntry)dictionary[key]).Where(x => x != null))
                entry.RegisterPostEvictionCallback(token.PostEvictionCallback);
            return token;
        }

        /// <summary>
        /// Forces the absolute expiration.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="delay">The delay.</param>
        /// <returns>MemoryCacheEntryOptions.</returns>
        public static MemoryCacheEntryOptions ForceAbsoluteExpiration(this MemoryCacheEntryOptions options, TimeSpan delay) =>
            options.AddExpirationToken(new CancellationChangeToken(new CancellationTokenSource(delay).Token));
    }
}
