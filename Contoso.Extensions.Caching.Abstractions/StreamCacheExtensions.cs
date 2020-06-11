using System;
using System.Threading;
using System.Threading.Tasks;
using IOStream = System.IO.Stream;

namespace Contoso.Extensions.Caching.Stream
{
    /// <summary>
    /// Extension methods for setting data in an <see cref="IStreamCache" />.
    /// </summary>
    public static class StreamCacheExtensions
    {
        /// <summary>
        /// Sets a stream in the specified cache with the specified key.
        /// </summary>
        /// <param name="cache">The cache in which to store the data.</param>
        /// <param name="key">The key to store the data in.</param>
        /// <param name="value">The data to store in the cache.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="key"/> or <paramref name="value"/> is null.</exception>
        public static void Set(this IStreamCache cache, string key, IOStream value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            cache.Set(key, value, new StreamCacheEntryOptions());
        }

        /// <summary>
        /// Asynchronously sets a stream in the specified cache with the specified key.
        /// </summary>
        /// <param name="cache">The cache in which to store the data.</param>
        /// <param name="key">The key to store the data in.</param>
        /// <param name="value">The data to store in the cache.</param>
        /// <param name="token">Optional. A <see cref="CancellationToken" /> to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous set operation.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="key"/> or <paramref name="value"/> is null.</exception>
        public static Task SetAsync(this IStreamCache cache, string key, IOStream value, CancellationToken token = default)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return cache.SetAsync(key, value, new StreamCacheEntryOptions(), token);
        }
    }
}