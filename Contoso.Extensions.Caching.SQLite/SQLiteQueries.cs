using System;

namespace Contoso.Extensions.Caching.SQLite
{
    internal class SQLiteQueries
    {
        const string TableInfoFormat =
            "SELECT name " +
            "FROM sqlite_master " +
            "WHERE type = 'table' AND name = '{0}';";

        const string UpdateCacheItemFormat =
            "UPDATE {0} " +
            "SET ExpiresAtTime = " +
                "(CASE " +
                    "WHEN (julianday(@UtcNow)-julianday(AbsoluteExpiration))*86400.0 <= SlidingExpirationInSeconds THEN AbsoluteExpiration " +
                    "ELSE datetime(@UtcNow, '+'||SlidingExpirationInSeconds||' seconds') " + 
                "END) " +
            "WHERE Id = @Id " +
            "AND @UtcNow <= ExpiresAtTime " +
            "AND SlidingExpirationInSeconds IS NOT NULL " +
            "AND (AbsoluteExpiration IS NULL OR AbsoluteExpiration <> ExpiresAtTime);";

        const string GetCacheItemFormat =
            "SELECT Id, ExpiresAtTime, SlidingExpirationInSeconds, AbsoluteExpiration, Value " +
            "FROM {0} WHERE Id = @Id AND @UtcNow <= ExpiresAtTime;";

        string SetCacheItemFormat(DateTimeOffset utcNow, TimeSpan? slidingExpirationInSeconds, DateTimeOffset? absoluteExpiration, out DateTimeOffset? expiresAtTime)
        {
            expiresAtTime = slidingExpirationInSeconds == null ? absoluteExpiration : utcNow.AddSeconds(slidingExpirationInSeconds.Value.TotalSeconds);
            return "UPDATE {0} SET Value = @Value, ExpiresAtTime = @ExpiresAtTime," +
                "SlidingExpirationInSeconds = @SlidingExpirationInSeconds, AbsoluteExpiration = @AbsoluteExpiration " +
                "WHERE Id = @Id; " +
                "INSERT INTO {0} " +
                "(Id, Value, ExpiresAtTime, SlidingExpirationInSeconds, AbsoluteExpiration) " +
                "SELECT @Id, @Value, @ExpiresAtTime, @SlidingExpirationInSeconds, @AbsoluteExpiration " +
                "WHERE changes() = 0;";
        }

        const string DeleteCacheItemFormat = "DELETE FROM {0} WHERE Id = @Id;";

        public const string DeleteExpiredCacheItemsFormat = "DELETE FROM {0} WHERE @UtcNow > ExpiresAtTime;";

        public SQLiteQueries(string tableName)
        {
            var tableNameEscaped = DelimitIdentifier(tableName);
            // when retrieving an item, we do an UPDATE first and then a SELECT
            GetCacheItem = string.Format(UpdateCacheItemFormat + GetCacheItemFormat, tableNameEscaped);
            GetCacheItemWithoutValue = string.Format(UpdateCacheItemFormat, tableNameEscaped);
            DeleteCacheItem = string.Format(DeleteCacheItemFormat, tableNameEscaped);
            DeleteExpiredCacheItems = string.Format(DeleteExpiredCacheItemsFormat, tableNameEscaped);
            SetCacheItem = (utcNow, slidingExpirationInSeconds, absoluteExpiration) => (string.Format(SetCacheItemFormat(utcNow, slidingExpirationInSeconds, absoluteExpiration, out var expiresAtTime), tableNameEscaped), expiresAtTime);
            TableInfo = string.Format(TableInfoFormat, EscapeLiteral(tableName));
        }

        public string TableInfo { get; }

        public string GetCacheItem { get; }

        public string GetCacheItemWithoutValue { get; }

        public Func<DateTimeOffset, TimeSpan?, DateTimeOffset?, (string query, DateTimeOffset? expiresAtTime)> SetCacheItem { get; }

        public string DeleteCacheItem { get; }

        public string DeleteExpiredCacheItems { get; }

        string DelimitIdentifier(string identifier) => "[" + identifier.Replace("]", "]]") + "]";

        string EscapeLiteral(string literal) => literal.Replace("'", "''");
    }
}
