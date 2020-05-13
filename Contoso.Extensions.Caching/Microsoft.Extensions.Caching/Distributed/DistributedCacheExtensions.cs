using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Microsoft.Extensions.Caching.Distributed
{
    public static class DistributedCacheExtensions
    {
        /// <summary>
        /// Expire the cache entry if the given <see cref="IChangeToken"/> expires.
        /// </summary>
        /// <param name="options">The <see cref="MemoryCacheEntryOptions"/>.</param>
        /// <param name="expirationToken">The <see cref="IChangeToken"/> that causes the cache entry to expire.</param>
        /// <returns>The <see cref="MemoryCacheEntryOptions"/> so that additional calls can be chained.</returns>
        public static DistributedCacheEntryOptions2 AddExpirationToken(this DistributedCacheEntryOptions2 options, IChangeToken expirationToken)
        {
            if (expirationToken == null)
                throw new ArgumentNullException(nameof(expirationToken));
            options.ExpirationTokens.Add(expirationToken);
            return options;
        }

        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if [contains] [the specified key]; otherwise, <c>false</c>.</returns>
        /// <exception cref="NotSupportedException"></exception>
        public static bool Contains(this IDistributedCache cache, string key) => throw new NotSupportedException();

        /// <summary>
        /// Sets the cache entry change expiration.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="key">The key.</param>
        /// <returns>DistributedCacheEntryOptions2.</returns>
        public static DistributedCacheEntryOptions2 SetCacheEntryChangeExpiration(this DistributedCacheEntryOptions2 options, IDistributedCache cache, string key)
        {
            options.AddExpirationToken(MakeCacheEntryChangeToken(cache, new[] { key }));
            return options;
        }

        /// <summary>
        /// Sets the file watch expiration.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>DistributedCacheEntryOptions2.</returns>
        public static DistributedCacheEntryOptions2 SetFileWatchExpiration(this DistributedCacheEntryOptions2 options, string fileName)
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
        /// <exception cref="NotSupportedException"></exception>
        public static IChangeToken MakeCacheEntryChangeToken(this IDistributedCache cache, IEnumerable<string> keys) => throw new NotSupportedException();

        /// <summary>
        /// Forces the absolute expiration.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="delay">The delay.</param>
        /// <returns>DistributedCacheEntryOptions.</returns>
        public static DistributedCacheEntryOptions2 ForceAbsoluteExpiration(this DistributedCacheEntryOptions2 options, TimeSpan delay) =>
            options.AddExpirationToken(new CancellationChangeToken(new CancellationTokenSource(delay).Token));
    }
}
