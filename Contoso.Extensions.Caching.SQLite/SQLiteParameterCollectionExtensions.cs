using System;
using System.Data;
using System.Data.SQLite;

namespace Contoso.Extensions.Caching.SQLite
{
    internal static class SQLiteParameterCollectionExtensions
    {
        public const int DefaultValueColumnWidth = 8000;
        public const int CacheItemIdColumnWidth = 449;

        public static SQLiteParameterCollection AddCacheItemId(this SQLiteParameterCollection parameters, string value) =>
            parameters.AddWithValue(Columns.Names.CacheItemId, DbType.String, CacheItemIdColumnWidth, value);

        public static SQLiteParameterCollection AddCacheItemValue(this SQLiteParameterCollection parameters, byte[] value) =>
            value != null && value.Length < DefaultValueColumnWidth
                ? parameters.AddWithValue(Columns.Names.CacheItemValue, DbType.Binary, DefaultValueColumnWidth, value)
                : throw new ArgumentOutOfRangeException(nameof(value));

        public static SQLiteParameterCollection AddSlidingExpirationInSeconds(this SQLiteParameterCollection parameters, TimeSpan? value) =>
            parameters.AddWithValue(Columns.Names.SlidingExpirationInSeconds, DbType.Int64, value.HasValue ? (object)value.Value.TotalSeconds : DBNull.Value);

        public static SQLiteParameterCollection AddAbsoluteExpiration(this SQLiteParameterCollection parameters, DateTimeOffset? utcTime) =>
            parameters.AddWithValue(Columns.Names.AbsoluteExpiration, DbType.DateTimeOffset, utcTime.HasValue ? (object)utcTime.Value : DBNull.Value);

        public static SQLiteParameterCollection AddWithValue(this SQLiteParameterCollection parameters, string parameterName, DbType dbType, object value)
        {
            var parameter = new SQLiteParameter(parameterName, dbType) { Value = value };
            parameters.Add(parameter);
            //parameter.ResetSqlDbType();
            return parameters;
        }

        public static SQLiteParameterCollection AddWithValue(this SQLiteParameterCollection parameters, string parameterName, DbType dbType, int size, object value)
        {
            var parameter = new SQLiteParameter(parameterName, dbType, size) { Value = value };
            parameters.Add(parameter);
            //parameter.ResetSqlDbType();
            return parameters;
        }
    }
}
