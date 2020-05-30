using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Internal;
using System;
using System.Data;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;

namespace Contoso.Extensions.Caching.SQLite
{
    internal interface IDatabaseOperations
    {
        byte[] GetCacheItem(string key);
        Task<byte[]> GetCacheItemAsync(string key, CancellationToken token = default);
        void RefreshCacheItem(string key);
        Task RefreshCacheItemAsync(string key, CancellationToken token = default);
        void DeleteCacheItem(string key);
        Task DeleteCacheItemAsync(string key, CancellationToken token = default);
        void SetCacheItem(string key, byte[] value, DistributedCacheEntryOptions options);
        Task SetCacheItemAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default);
        void DeleteExpiredCacheItems();
    }

    internal class DatabaseOperations : IDatabaseOperations
    {
        const int DuplicateKeyErrorId = 19; // constraint violated
        protected const string GetTableErrorText = "Could not retrieve information of table with name '{0}'. Make sure you have the table setup and try again. Connection string: {1}";

        public DatabaseOperations(string connectionString, string tableName, ISystemClock systemClock)
        {
            ConnectionString = connectionString;
            TableName = tableName;
            SystemClock = systemClock;
            SqlQueries = new SQLiteQueries(tableName);
        }

        protected SQLiteQueries SqlQueries { get; }

        protected string ConnectionString { get; }

        protected string TableName { get; }

        protected ISystemClock SystemClock { get; }

        public void DeleteCacheItem(string key)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            using (var command = new SQLiteCommand(SqlQueries.DeleteCacheItem, connection))
            {
                command.Parameters.AddCacheItemId(key);

                connection.Open();

                command.ExecuteNonQuery();
            }
        }

        public async Task DeleteCacheItemAsync(string key, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            using (var connection = new SQLiteConnection(ConnectionString))
            using (var command = new SQLiteCommand(SqlQueries.DeleteCacheItem, connection))
            {
                command.Parameters.AddCacheItemId(key);

                await connection.OpenAsync(token).ConfigureAwait(false);

                await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            }
        }

        public virtual byte[] GetCacheItem(string key) => GetCacheItem(key, includeValue: true);

        public virtual async Task<byte[]> GetCacheItemAsync(string key, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            return await GetCacheItemAsync(key, includeValue: true, token: token).ConfigureAwait(false);
        }

        public void RefreshCacheItem(string key) => GetCacheItem(key, includeValue: false);

        public async Task RefreshCacheItemAsync(string key, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            await GetCacheItemAsync(key, includeValue: false, token: token).ConfigureAwait(false);
        }

        public virtual void DeleteExpiredCacheItems()
        {
            var utcNow = SystemClock.UtcNow;

            using (var connection = new SQLiteConnection(ConnectionString))
            using (var command = new SQLiteCommand(SqlQueries.DeleteExpiredCacheItems, connection))
            {
                command.Parameters.AddWithValue("UtcNow", DbType.DateTimeOffset, utcNow.ToString("o"));

                connection.Open();

                var effectedRowCount = command.ExecuteNonQuery();
            }
        }

        public virtual void SetCacheItem(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            var utcNow = SystemClock.UtcNow;

            var absoluteExpiration = GetAbsoluteExpiration(utcNow, options);
            ValidateOptions(options.SlidingExpiration, absoluteExpiration);

            var (query, expiresAtTime) = SqlQueries.SetCacheItem(utcNow, options.SlidingExpiration, absoluteExpiration);
            using (var connection = new SQLiteConnection(ConnectionString))
            using (var upsertCommand = new SQLiteCommand(query, connection))
            {
                upsertCommand.Parameters
                    .AddCacheItemId(key)
                    .AddCacheItemValue(value)
                    .AddSlidingExpirationInSeconds(options.SlidingExpiration)
                    .AddAbsoluteExpiration(absoluteExpiration)
                    .AddWithValue("ExpiresAtTime", DbType.DateTimeOffset, expiresAtTime.ToString("o"));

                connection.Open();

                try { upsertCommand.ExecuteNonQuery(); }
                catch (SQLiteException ex)
                {
                    // There is a possibility that multiple requests can try to add the same item to the cache, in which case we receive a 'duplicate key' exception on the primary key column.
                    if (IsDuplicateKeyException(ex)) { }
                    else throw;
                }
            }
        }

        public virtual async Task SetCacheItemAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var utcNow = SystemClock.UtcNow;

            var absoluteExpiration = GetAbsoluteExpiration(utcNow, options);
            ValidateOptions(options.SlidingExpiration, absoluteExpiration);

            var (query, expiresAtTime) = SqlQueries.SetCacheItem(utcNow, options.SlidingExpiration, absoluteExpiration);
            using (var connection = new SQLiteConnection(ConnectionString))
            using (var upsertCommand = new SQLiteCommand(query, connection))
            {
                upsertCommand.Parameters
                    .AddCacheItemId(key)
                    .AddCacheItemValue(value)
                    .AddSlidingExpirationInSeconds(options.SlidingExpiration)
                    .AddAbsoluteExpiration(absoluteExpiration)
                    .AddWithValue("ExpiresAtTime", DbType.DateTimeOffset, expiresAtTime.ToString("o"));

                await connection.OpenAsync(token).ConfigureAwait(false);

                try { await upsertCommand.ExecuteNonQueryAsync(token).ConfigureAwait(false); }
                catch (SQLiteException ex)
                {
                    // There is a possibility that multiple requests can try to add the same item to the cache, in which case we receive a 'duplicate key' exception on the primary key column.
                    if (IsDuplicateKeyException(ex)) { }
                    else throw;
                }
            }
        }

        protected virtual byte[] GetCacheItem(string key, bool includeValue)
        {
            var utcNow = SystemClock.UtcNow;

            var query = includeValue
                ? SqlQueries.GetCacheItem
                : SqlQueries.GetCacheItemWithoutValue;

            byte[] value = null;
            TimeSpan? slidingExpiration;
            DateTimeOffset? absoluteExpiration;
            DateTimeOffset expirationTime;
            using (var connection = new SQLiteConnection(ConnectionString))
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters
                    .AddCacheItemId(key)
                    .AddWithValue("UtcNow", DbType.DateTimeOffset, utcNow.ToString("o"));

                connection.Open();

                using (var reader = command.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleRow | CommandBehavior.SingleResult))
                    if (reader.Read())
                    {
                        var id = reader.GetFieldValue<string>(Columns.Indexes.CacheItemIdIndex);

                        expirationTime = DateTime.SpecifyKind(reader.GetFieldValue<DateTime>(Columns.Indexes.ExpiresAtTimeIndex), DateTimeKind.Utc);

                        if (!reader.IsDBNull(Columns.Indexes.SlidingExpirationInSecondsIndex))
                            slidingExpiration = TimeSpan.FromSeconds(reader.GetFieldValue<long>(Columns.Indexes.SlidingExpirationInSecondsIndex));

                        if (!reader.IsDBNull(Columns.Indexes.AbsoluteExpirationIndex))
                            absoluteExpiration = DateTime.SpecifyKind(reader.GetFieldValue<DateTime>(Columns.Indexes.AbsoluteExpirationIndex), DateTimeKind.Utc);

                        if (includeValue)
                            value = reader.GetFieldValue<byte[]>(Columns.Indexes.CacheItemValueIndex);
                    }
                    else
                        return null;
            }
            return value;
        }

        protected virtual async Task<byte[]> GetCacheItemAsync(string key, bool includeValue, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var utcNow = SystemClock.UtcNow;

            var query = includeValue
                ? SqlQueries.GetCacheItem
                : SqlQueries.GetCacheItemWithoutValue;

            byte[] value = null;
            TimeSpan? slidingExpiration;
            DateTimeOffset? absoluteExpiration;
            DateTimeOffset expirationTime;
            using (var connection = new SQLiteConnection(ConnectionString))
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters
                    .AddCacheItemId(key)
                    .AddWithValue("UtcNow", DbType.DateTimeOffset, utcNow.ToString("o"));

                await connection.OpenAsync(token).ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess | CommandBehavior.SingleRow | CommandBehavior.SingleResult, token).ConfigureAwait(false))
                    if (await reader.ReadAsync(token).ConfigureAwait(false))
                    {
                        var id = reader.GetFieldValue<string>(Columns.Indexes.CacheItemIdIndex);

                        expirationTime = DateTime.SpecifyKind(reader.GetFieldValue<DateTime>(Columns.Indexes.ExpiresAtTimeIndex), DateTimeKind.Utc);

                        if (!reader.IsDBNull(Columns.Indexes.SlidingExpirationInSecondsIndex))
                            slidingExpiration = TimeSpan.FromSeconds(reader.GetFieldValue<long>(Columns.Indexes.SlidingExpirationInSecondsIndex));

                        if (!reader.IsDBNull(Columns.Indexes.AbsoluteExpirationIndex))
                            absoluteExpiration = DateTime.SpecifyKind(reader.GetFieldValue<DateTime>(Columns.Indexes.AbsoluteExpirationIndex), DateTimeKind.Utc);

                        if (includeValue)
                            value = await reader.GetFieldValueAsync<byte[]>(Columns.Indexes.CacheItemValueIndex, token).ConfigureAwait(false);
                    }
                    else
                        return null;
            }
            return value;
        }

        protected bool IsDuplicateKeyException(SQLiteException ex) => ex.ErrorCode == DuplicateKeyErrorId;

        protected DateTimeOffset? GetAbsoluteExpiration(DateTimeOffset utcNow, DistributedCacheEntryOptions options)
        {
            // calculate absolute expiration
            DateTimeOffset? absoluteExpiration = null;
            if (options.AbsoluteExpirationRelativeToNow.HasValue)
                absoluteExpiration = utcNow.Add(options.AbsoluteExpirationRelativeToNow.Value);
            else if (options.AbsoluteExpiration.HasValue)
            {
                if (options.AbsoluteExpiration.Value <= utcNow)
                    throw new InvalidOperationException("The absolute expiration value must be in the future.");
                absoluteExpiration = options.AbsoluteExpiration.Value;
            }
            return absoluteExpiration;
        }

        protected void ValidateOptions(TimeSpan? slidingExpiration, DateTimeOffset? absoluteExpiration)
        {
            if (!slidingExpiration.HasValue && !absoluteExpiration.HasValue)
                throw new InvalidOperationException("Either absolute or sliding expiration needs to be provided.");
        }
    }
}
