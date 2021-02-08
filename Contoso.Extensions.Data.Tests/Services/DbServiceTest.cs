using Microsoft.Extensions.Caching.SqlServer;
using Xunit;

namespace Contoso.Extensions.Services
{
    public class DbServiceTest
    {
        static readonly IDbService _dbService = new DbService();
        static DbServiceTest() => Config.Setup();

        [Fact]
        public void GetConnection()
        {
        }

        [Fact]
        public void GetConnectionString()
        {
            var connectionString = _dbService.GetConnectionString();
            Assert.Contains("Access Token", connectionString);
        }

        [Fact]
        public void SqlServerCache()
        {
            var cache = new SqlServerCache(new SqlServerCacheOptions
            {
                ConnectionString = _dbService.GetConnectionString(),
                SchemaName = "deg",
                TableName = "_ObjectCache",
            });
            var test = cache.Get("Test");
        }
    }
}
