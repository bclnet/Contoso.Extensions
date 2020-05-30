using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
[assembly: InternalsVisibleTo("Contoso.Extensions.Caching.SQLite.Tests")]

namespace Contoso.Extensions.Caching.SQLite
{
    /// <summary>
    /// Distributed cache implementation using a SQLite database.
    /// </summary>
    public class SQLiteCache : IDistributedCache
    {
        static readonly TimeSpan MinimumExpiredItemsDeletionInterval = TimeSpan.FromMinutes(5);
        static readonly TimeSpan DefaultExpiredItemsDeletionInterval = TimeSpan.FromMinutes(30);

        readonly IDatabaseOperations _dbOperations;
        readonly ISystemClock _systemClock;
        readonly TimeSpan _expiredItemsDeletionInterval;
        DateTimeOffset _lastExpirationScan;
        readonly Action _deleteExpiredCachedItemsDelegate;
        readonly TimeSpan _defaultSlidingExpiration;

        public SQLiteCache(IOptions<SQLiteCacheOptions> options)
        {
            var cacheOptions = options.Value;
            if (string.IsNullOrEmpty(cacheOptions.ConnectionString))
                throw new ArgumentException($"{nameof(SQLiteCacheOptions.ConnectionString)} cannot be empty or null.");
            if (string.IsNullOrEmpty(cacheOptions.TableName))
                throw new ArgumentException($"{nameof(SQLiteCacheOptions.TableName)} cannot be empty or null.");
            if (cacheOptions.ExpiredItemsDeletionInterval.HasValue && cacheOptions.ExpiredItemsDeletionInterval.Value < MinimumExpiredItemsDeletionInterval)
                throw new ArgumentException($"{nameof(SQLiteCacheOptions.ExpiredItemsDeletionInterval)} cannot be less than the minimum value of {MinimumExpiredItemsDeletionInterval.TotalMinutes} minutes.");
            if (cacheOptions.DefaultSlidingExpiration <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(cacheOptions.DefaultSlidingExpiration), cacheOptions.DefaultSlidingExpiration, "The sliding expiration value must be positive.");

            _systemClock = cacheOptions.SystemClock ?? new SystemClock();
            _expiredItemsDeletionInterval = cacheOptions.ExpiredItemsDeletionInterval ?? DefaultExpiredItemsDeletionInterval;
            _deleteExpiredCachedItemsDelegate = DeleteExpiredCacheItems;
            _defaultSlidingExpiration = cacheOptions.DefaultSlidingExpiration;
            _dbOperations = new DatabaseOperations(cacheOptions.ConnectionString, cacheOptions.TableName, _systemClock);
        }

        public byte[] Get(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            
            var value = _dbOperations.GetCacheItem(key);

            ScanForExpiredItemsIfRequired();

            return value;
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            token.ThrowIfCancellationRequested();

            var value = await _dbOperations.GetCacheItemAsync(key, token).ConfigureAwait(false);

            ScanForExpiredItemsIfRequired();

            return value;
        }

        public void Refresh(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            _dbOperations.RefreshCacheItem(key);

            ScanForExpiredItemsIfRequired();
        }

        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            token.ThrowIfCancellationRequested();

            await _dbOperations.RefreshCacheItemAsync(key, token).ConfigureAwait(false);

            ScanForExpiredItemsIfRequired();
        }

        public void Remove(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            _dbOperations.DeleteCacheItem(key);

            ScanForExpiredItemsIfRequired();
        }

        public async Task RemoveAsync(string key, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            
            token.ThrowIfCancellationRequested();

            await _dbOperations.DeleteCacheItemAsync(key, token).ConfigureAwait(false);

            ScanForExpiredItemsIfRequired();
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            GetOptions(ref options);

            _dbOperations.SetCacheItem(key, value, options);

            ScanForExpiredItemsIfRequired();
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

            GetOptions(ref options);

            await _dbOperations.SetCacheItemAsync(key, value, options, token).ConfigureAwait(false);

            ScanForExpiredItemsIfRequired();
        }

        // Called by multiple actions to see how long it's been since we last checked for expired items.
        // If sufficient time has elapsed then a scan is initiated on a background task.
        void ScanForExpiredItemsIfRequired()
        {
            var utcNow = _systemClock.UtcNow;
            // TODO: Multiple threads could trigger this scan which leads to multiple calls to database.
            if ((utcNow - _lastExpirationScan) > _expiredItemsDeletionInterval)
            {
                _lastExpirationScan = utcNow;
                Task.Run(_deleteExpiredCachedItemsDelegate);
            }
        }

        void DeleteExpiredCacheItems() => _dbOperations.DeleteExpiredCacheItems();

        void GetOptions(ref DistributedCacheEntryOptions options)
        {
            if (!options.AbsoluteExpiration.HasValue && !options.AbsoluteExpirationRelativeToNow.HasValue && !options.SlidingExpiration.HasValue)
                options = new DistributedCacheEntryOptions()
                {
                    SlidingExpiration = _defaultSlidingExpiration
                };
        }
    }
}
