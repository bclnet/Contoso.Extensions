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
    public class MemoryStreamCache : IStreamCache
    {
        static readonly Task CompletedTask = Task.FromResult<object>(null);

        internal readonly IMemoryCache _memCache;
        readonly MemoryStreamCacheOptions _options;

        public MemoryStreamCache(IOptions<MemoryStreamCacheOptions> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _options = options.Value;
            _memCache = new MemoryCache(options.Value);
        }

        public IOStream Get(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return (IOStream)_memCache.Get(key);
        }

        public Task<IOStream> GetAsync(string key, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            token.ThrowIfCancellationRequested();

            return Task.FromResult(Get(key));
        }

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

        public void Refresh(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            _memCache.TryGetValue(key, out var _);
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            token.ThrowIfCancellationRequested();

            Refresh(key);
            return CompletedTask;
        }

        public void Remove(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            _memCache.Remove(key);
        }

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