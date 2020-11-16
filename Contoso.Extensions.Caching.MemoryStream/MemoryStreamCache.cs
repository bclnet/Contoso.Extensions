using Contoso.Extensions.Caching.Stream;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using IOStream = System.IO.Stream;

namespace Contoso.Extensions.Caching.MemoryStream
{
    /// <summary>
    /// MemoryStreamCache
    /// </summary>
    /// <seealso cref="Contoso.Extensions.Caching.Stream.IStreamCache" />
    public class MemoryStreamCache : IStreamCache
    {
        static readonly Task CompletedTask = Task.FromResult<object>(null);

        internal readonly IMemoryCache _memCache;
        readonly MemoryStreamCacheOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStreamCache"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="ArgumentNullException">options</exception>
        public MemoryStreamCache(IOptions<MemoryStreamCacheOptions> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _options = options.Value;
            _memCache = new MemoryCache(options.Value);
        }

        /// <summary>
        /// Gets a value with the given key.
        /// </summary>
        /// <param name="key">A long identifying the requested value.</param>
        /// <returns>
        /// The located value or null.
        /// </returns>
        /// <exception cref="ArgumentNullException">key</exception>
        public IOStream Get(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return (IOStream)_memCache.Get(key);
        }

        /// <summary>
        /// Gets a value with the given key.
        /// </summary>
        /// <param name="key">A long? identifying the requested value.</param>
        /// <param name="token">Optional. The <see cref="T:System.Threading.CancellationToken" /> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>
        /// The <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation, containing the located value or null.
        /// </returns>
        /// <exception cref="ArgumentNullException">key</exception>
        public Task<IOStream> GetAsync(string key, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            token.ThrowIfCancellationRequested();

            return Task.FromResult(Get(key));
        }

        /// <summary>
        /// Sets a value with the given key.
        /// </summary>
        /// <param name="key">A long? identifying the requested value.</param>
        /// <param name="value">The value to set in the cache.</param>
        /// <param name="options">The cache options for the value.</param>
        /// <exception cref="ArgumentNullException">key or value or options</exception>
        public void Set(string key, IOStream value, StreamCacheEntryOptions options)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var entryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = options.AbsoluteExpiration,
                AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = options.SlidingExpiration,
                Size = value is StreamWithHeader z ? z.TotalLength : value.Length,
            };

            _memCache.Set(key, value, entryOptions);
        }

        /// <summary>
        /// Sets the value with the given key.
        /// </summary>
        /// <param name="key">A long identifying the requested value.</param>
        /// <param name="value">The value to set in the cache.</param>
        /// <param name="options">The cache options for the value.</param>
        /// <param name="token">Optional. The <see cref="T:System.Threading.CancellationToken" /> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>
        /// The <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">key or value or options</exception>
        public Task SetAsync(string key, IOStream value, StreamCacheEntryOptions options, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            token.ThrowIfCancellationRequested();

            Set(key, value, options);
            return CompletedTask;
        }

        /// <summary>
        /// Refreshes a value in the cache based on its key, resetting its sliding expiration timeout (if any).
        /// </summary>
        /// <param name="key">A string identifying the requested calue.</param>
        /// <exception cref="ArgumentNullException">key</exception>
        public void Refresh(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            _memCache.TryGetValue(key, out var _);
        }

        /// <summary>
        /// Refreshes a value in the cache based on its key, resetting its sliding expiration timeout (if any).
        /// </summary>
        /// <param name="key">A long identifying the requested value.</param>
        /// <param name="token">Optional. The <see cref="T:System.Threading.CancellationToken" /> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>
        /// The <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">key</exception>
        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            token.ThrowIfCancellationRequested();

            Refresh(key);
            return CompletedTask;
        }

        /// <summary>
        /// Removes the value with the given key.
        /// </summary>
        /// <param name="key">A long identifying the requested value.</param>
        /// <exception cref="ArgumentNullException">key</exception>
        public void Remove(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            _memCache.Remove(key);
        }

        /// <summary>
        /// Removes the value with the given key.
        /// </summary>
        /// <param name="key">A long identifying the requested value.</param>
        /// <param name="token">Optional. The <see cref="T:System.Threading.CancellationToken" /> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>
        /// The <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">key</exception>
        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            token.ThrowIfCancellationRequested();

            Remove(key);
            return CompletedTask;
        }
    }
}