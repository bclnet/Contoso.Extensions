using System;

namespace Contoso.Extensions.Caching.SqliteConfig.Tools
{
    internal class SqlQueries
    {
        const string CreateTableFormat = "CREATE TABLE {0}(" +
           "Id nvarchar(449) primary key not null, " +
           "Value blob not null, " +
           "ExpiresAtTime datetime not null, " +
           "SlidingExpirationInSeconds bigint null, " +
           "AbsoluteExpiration datetime null" +
           ")";

        const string CreateIndexOnExpirationTimeFormat
           = "CREATE INDEX Index_ExpiresAtTime ON {0}(ExpiresAtTime)";

        const string TableInfoFormat =
            "SELECT name " +
            "FROM sqlite_master " +
            "WHERE type = 'table' AND name = '{0}'";

        public SqlQueries(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Table name cannot be empty or null");

            var tableNameEscaped = DelimitIdentifier(tableName);
            CreateTable = string.Format(CreateTableFormat, tableNameEscaped);
            CreateIndexOnExpirationTime = string.Format(CreateIndexOnExpirationTimeFormat, tableNameEscaped);
            TableInfo = string.Format(TableInfoFormat, EscapeLiteral(tableName));
        }

        public string CreateTable { get; }

        public string CreateIndexOnExpirationTime { get; }

        public string TableInfo { get; }

        string DelimitIdentifier(string identifier) => $"[{identifier.Replace("]", "]]")}]";

        string EscapeLiteral(string literal) => literal.Replace("'", "''");
    }
}