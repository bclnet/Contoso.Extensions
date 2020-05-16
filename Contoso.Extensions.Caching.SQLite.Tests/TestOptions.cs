using Microsoft.Extensions.Options;

namespace Contoso.Extensions.Caching.SQLite
{
    internal class TestSqlServerCacheOptions : IOptions<SQLiteCacheOptions>
    {
        public TestSqlServerCacheOptions(SQLiteCacheOptions innerOptions) => Value = innerOptions;

        public SQLiteCacheOptions Value { get; }
    }
}
