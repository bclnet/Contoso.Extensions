using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Contoso.Extensions.Caching.FileSystem
{
    public class FileSystemCache : IDistributedCache, IDisposable
    {
        const string AbsoluteExpirationKey = "absexp";
        const string SlidingExpirationKey = "sldexp";
        const string DataKey = "data";
        const long NotPresent = -1;

        Database _cache;
        readonly FileSystemCacheOptions _options;
        readonly string _instance;
        readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        public FileSystemCache(IOptions<FileSystemCacheOptions> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            _options = options.Value;
            // This allows partitioning a single backend cache for use with multiple apps/services.
            _instance = _options.InstanceName ?? string.Empty;
        }

        public void Dispose() => _cache?.Close();

        public byte[] Get(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            return GetAndRefresh(key, getData: true);
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            token.ThrowIfCancellationRequested();
            return await GetAndRefreshAsync(key, getData: true, token: token).ConfigureAwait(false);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            Connect();
            var creationTime = DateTimeOffset.UtcNow;
            var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);
            //var result = _cache.Set(_instance + key, value,
            //    new object[]
            //    {
            //        absoluteExpiration?.Ticks ?? NotPresent,
            //        options.SlidingExpiration?.Ticks ?? NotPresent,
            //        GetExpirationInSeconds(creationTime, absoluteExpiration, options) ?? NotPresent,
            //    });
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token).ConfigureAwait(false);
            var creationTime = DateTimeOffset.UtcNow;
            var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);
            //await _cache.SetAsync(_instance + key, value,
            //    new object[]
            //    {
            //        absoluteExpiration?.Ticks ?? NotPresent,
            //        options.SlidingExpiration?.Ticks ?? NotPresent,
            //        GetExpirationInSeconds(creationTime, absoluteExpiration, options) ?? NotPresent,
            //    }).ConfigureAwait(false);
        }

        public void Refresh(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            GetAndRefresh(key, getData: false);
        }

        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
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
                    _cache = Database.GetDatabase(_options.ConfigurationOptions ?? new DatabaseOptions(_options.Configuration));
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
                    _cache = await Database.GetDatabaseAsync(_options.ConfigurationOptions ?? new DatabaseOptions(_options.Configuration)).ConfigureAwait(false);
            }
            finally { _connectionLock.Release(); }
        }

        byte[] GetAndRefresh(string key, bool getData)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            Connect();

            // This also resets the LRU status as desired.
            // TODO: Can this be done in one operation on the server side? Probably, the trick would just be the DateTimeOffset math.
            //RedisValue[] results;
            //if (getData)
            //    results = _cache.HashMemberGet(_instance + key, AbsoluteExpirationKey, SlidingExpirationKey, DataKey);
            //else
            //    results = _cache.HashMemberGet(_instance + key, AbsoluteExpirationKey, SlidingExpirationKey);

            //// TODO: Error handling
            //if (results.Length >= 2)
            //{
            //    MapMetadata(results, out DateTimeOffset? absExpr, out TimeSpan? sldExpr);
            //    Refresh(key, absExpr, sldExpr);
            //}

            //if (results.Length >= 3 && results[2].HasValue)
            //    return results[2];
            return null;
        }

        async Task<byte[]> GetAndRefreshAsync(string key, bool getData, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token).ConfigureAwait(false);
            // This also resets the LRU status as desired.
            // TODO: Can this be done in one operation on the server side? Probably, the trick would just be the DateTimeOffset math.
            //RedisValue[] results;
            //if (getData)
            //    results = await _cache.HashMemberGetAsync(_instance + key, AbsoluteExpirationKey, SlidingExpirationKey, DataKey).ConfigureAwait(false);
            //else
            //    results = await _cache.HashMemberGetAsync(_instance + key, AbsoluteExpirationKey, SlidingExpirationKey).ConfigureAwait(false);
            //// TODO: Error handling
            //if (results.Length >= 2)
            //{
            //    MapMetadata(results, out DateTimeOffset? absExpr, out TimeSpan? sldExpr);
            //    await RefreshAsync(key, absExpr, sldExpr, token).ConfigureAwait(false);
            //}

            //if (results.Length >= 3 && results[2].HasValue)
            //    return results[2];
            return null;
        }

        public void Remove(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            Connect();
            _cache.KeyDelete(_instance + key);
            // TODO: Error handling
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            await ConnectAsync(token).ConfigureAwait(false);
            await _cache.KeyDeleteAsync(_instance + key).ConfigureAwait(false);
            // TODO: Error handling
        }

        void MapMetadata(object[] results, out DateTimeOffset? absoluteExpiration, out TimeSpan? slidingExpiration)
        {
            absoluteExpiration = null;
            slidingExpiration = null;
            var absoluteExpirationTicks = (long?)results[0];
            if (absoluteExpirationTicks.HasValue && absoluteExpirationTicks.Value != NotPresent)
                absoluteExpiration = new DateTimeOffset(absoluteExpirationTicks.Value, TimeSpan.Zero);
            var slidingExpirationTicks = (long?)results[1];
            if (slidingExpirationTicks.HasValue && slidingExpirationTicks.Value != NotPresent)
                slidingExpiration = new TimeSpan(slidingExpirationTicks.Value);
        }

        void Refresh(string key, DateTimeOffset? absExpr, TimeSpan? sldExpr)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            // Note Refresh has no effect if there is just an absolute expiration (or neither).
            TimeSpan? expr = null;
            if (sldExpr.HasValue)
            {
                if (absExpr.HasValue)
                {
                    var relExpr = absExpr.Value - DateTimeOffset.Now;
                    expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
                }
                else
                    expr = sldExpr;
                _cache.KeyExpire(_instance + key, expr);
                // TODO: Error handling
            }
        }

        async Task RefreshAsync(string key, DateTimeOffset? absExpr, TimeSpan? sldExpr, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            token.ThrowIfCancellationRequested();
            // Note Refresh has no effect if there is just an absolute expiration (or neither).
            TimeSpan? expr = null;
            if (sldExpr.HasValue)
            {
                if (absExpr.HasValue)
                {
                    var relExpr = absExpr.Value - DateTimeOffset.Now;
                    expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
                }
                else
                    expr = sldExpr;
                await _cache.KeyExpireAsync(_instance + key, expr).ConfigureAwait(false);
                // TODO: Error handling
            }
        }

        static long? GetExpirationInSeconds(DateTimeOffset creationTime, DateTimeOffset? absoluteExpiration, DistributedCacheEntryOptions options)
        {
            if (absoluteExpiration.HasValue && options.SlidingExpiration.HasValue)
                return (long)Math.Min(
                    (absoluteExpiration.Value - creationTime).TotalSeconds,
                    options.SlidingExpiration.Value.TotalSeconds);
            else if (absoluteExpiration.HasValue)
                return (long)(absoluteExpiration.Value - creationTime).TotalSeconds;
            else if (options.SlidingExpiration.HasValue)
                return (long)options.SlidingExpiration.Value.TotalSeconds;
            return null;
        }

        static DateTimeOffset? GetAbsoluteExpiration(DateTimeOffset creationTime, DistributedCacheEntryOptions options)
        {
            if (options.AbsoluteExpiration.HasValue && options.AbsoluteExpiration <= creationTime)
                throw new ArgumentOutOfRangeException(
                    nameof(DistributedCacheEntryOptions.AbsoluteExpiration),
                    options.AbsoluteExpiration.Value,
                    "The absolute expiration value must be in the future.");
            var absoluteExpiration = options.AbsoluteExpiration;
            if (options.AbsoluteExpirationRelativeToNow.HasValue)
                absoluteExpiration = creationTime + options.AbsoluteExpirationRelativeToNow;
            return absoluteExpiration;
        }
    }
}