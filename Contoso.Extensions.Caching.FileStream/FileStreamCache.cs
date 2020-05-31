using System;
using System.Threading;
using System.Threading.Tasks;
using Contoso.Extensions.Caching.Stream;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using IOStream = System.IO.Stream;

namespace Contoso.Extensions.Caching.FileStream
{
    public class FileStreamCache : IStreamCache, IDisposable
    {
        const long NotPresent = -1;

        Database _cache;
        readonly FileStreamCacheOptions _options;
        readonly string _instance;
        readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        public FileStreamCache(IOptions<FileStreamCacheOptions> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _options = options.Value;
            _instance = _options.InstanceName ?? string.Empty;
        }

        public void Dispose() => _cache?.Close();

        public IOStream Get(long key)
        {
            return GetAndRefresh(key, getData: true);
        }

        public async Task<IOStream> GetAsync(long key, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            return await GetAndRefreshAsync(key, getData: true, token: token).ConfigureAwait(false);
        }

        public long Set(long? key, IOStream value, StreamCacheEntryOptions options)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            Connect();

            var creationTime = DateTimeOffset.UtcNow;
            var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);

            var result = _cache.Set(key, new MetadataStream<byte[][]>(value,
                new byte[][]
                {
                    BitConverter.GetBytes(absoluteExpiration?.Ticks ?? NotPresent),
                    BitConverter.GetBytes(options.SlidingExpiration?.Ticks ?? NotPresent),
                }));

            return result;
        }

        public async Task<long> SetAsync(long? key, IOStream value, StreamCacheEntryOptions options, CancellationToken token = default)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            token.ThrowIfCancellationRequested();

            await ConnectAsync(token).ConfigureAwait(false);

            var creationTime = DateTimeOffset.UtcNow;
            var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);

            var result = await _cache.SetAsync(key, new MetadataStream<byte[][]>(value,
                new byte[][]
                {
                    BitConverter.GetBytes(absoluteExpiration?.Ticks ?? NotPresent),
                    BitConverter.GetBytes(options.SlidingExpiration?.Ticks ?? NotPresent),
                })).ConfigureAwait(false);

            return result;
        }

        public void Refresh(long key)
        {
            GetAndRefresh(key, getData: false);
        }

        public async Task RefreshAsync(long key, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            await GetAndRefreshAsync(key, getData: false, token: token).ConfigureAwait(false);
        }

        void Connect()
        {
            if (_cache != null)
                return;

            _connectionLock.Wait();
            try
            {
                if (_cache == null)
                    _cache = Database.GetDatabase(_options.ConfigurationOptions ?? new DatabaseOptions(_options.Configuration), _instance);
            }
            finally { _connectionLock.Release(); }
        }

        async Task ConnectAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (_cache != null)
                return;

            await _connectionLock.WaitAsync(token).ConfigureAwait(false);
            try
            {
                if (_cache == null)
                    _cache = await Database.GetDatabaseAsync(_options.ConfigurationOptions ?? new DatabaseOptions(_options.Configuration), _instance).ConfigureAwait(false);
            }
            finally { _connectionLock.Release(); }
        }

        IOStream GetAndRefresh(long key, bool getData)
        {
            Connect();

            var results = _cache.Get(key, getData: getData);

            MapMetadata(results.Metadata, out var absExpr, out var sldExpr);
            Refresh(key, absExpr, sldExpr);

            return results.Base;
        }

        async Task<IOStream> GetAndRefreshAsync(long key, bool getData, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            await ConnectAsync(token).ConfigureAwait(false);

            var results = await _cache.GetAsync(key, getData: getData).ConfigureAwait(false);

            MapMetadata(results.Metadata, out var absExpr, out var sldExpr);
            await RefreshAsync(key, absExpr, sldExpr).ConfigureAwait(false);

            return results.Base;
        }

        public void Remove(long key)
        {
            Connect();

            _cache.Delete(key);
        }

        public async Task RemoveAsync(long key, CancellationToken token = default)
        {
            await ConnectAsync(token).ConfigureAwait(false);

            await _cache.DeleteAsync(key).ConfigureAwait(false);
        }

        void MapMetadata(byte[][] meta, out DateTimeOffset? absoluteExpiration, out TimeSpan? slidingExpiration)
        {
            absoluteExpiration = null;
            slidingExpiration = null;
            var absoluteExpirationTicks = BitConverter.ToInt64(meta[0], 0);
            if (absoluteExpirationTicks != NotPresent)
                absoluteExpiration = new DateTimeOffset(absoluteExpirationTicks, TimeSpan.Zero);
            var slidingExpirationTicks = BitConverter.ToInt64(meta[1], 0);
            if (slidingExpirationTicks != NotPresent)
                slidingExpiration = new TimeSpan(slidingExpirationTicks);
        }

        void Refresh(long key, DateTimeOffset? absExpr, TimeSpan? sldExpr)
        {
            // Note Refresh has no effect if there is just an absolute expiration (or neither).
            if (sldExpr.HasValue)
            {
                TimeSpan? expr;
                if (absExpr.HasValue)
                {
                    var relExpr = absExpr.Value - DateTimeOffset.Now;
                    expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
                }
                else
                    expr = sldExpr;
                _cache.Delete(key, expr);
            }
        }

        async Task RefreshAsync(long key, DateTimeOffset? absExpr, TimeSpan? sldExpr, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            // Note Refresh has no effect if there is just an absolute expiration (or neither).
            if (sldExpr.HasValue)
            {
                TimeSpan? expr;
                if (absExpr.HasValue)
                {
                    var relExpr = absExpr.Value - DateTimeOffset.Now;
                    expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
                }
                else
                    expr = sldExpr;
                await _cache.DeleteAsync(key, expr).ConfigureAwait(false);
            }
        }

        static DateTimeOffset? GetAbsoluteExpiration(DateTimeOffset creationTime, StreamCacheEntryOptions options)
        {
            if (options.AbsoluteExpiration.HasValue && options.AbsoluteExpiration <= creationTime)
                throw new ArgumentOutOfRangeException(nameof(DistributedCacheEntryOptions.AbsoluteExpiration), options.AbsoluteExpiration.Value, "The absolute expiration value must be in the future.");
            var absoluteExpiration = options.AbsoluteExpiration;
            if (options.AbsoluteExpirationRelativeToNow.HasValue)
                absoluteExpiration = creationTime + options.AbsoluteExpirationRelativeToNow;
            return absoluteExpiration;
        }
    }
}