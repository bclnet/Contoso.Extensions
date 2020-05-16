using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Linq;
using Xunit;

namespace Contoso.Extensions.Caching.SQLite
{
    public class SQLiteCacheServicesExtensionsTest
    {
        [Fact]
        public void AddDistributedSQLiteCache_AddsAsSingleRegistrationService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            SQLiteCachingServicesExtensions.AddSQLiteCacheServices(services);

            // Assert
            var serviceDescriptor = Assert.Single(services);
            Assert.Equal(typeof(IDistributedCache), serviceDescriptor.ServiceType);
            Assert.Equal(typeof(SQLiteCache), serviceDescriptor.ImplementationType);
            Assert.Equal(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);
        }

        [Fact]
        public void AddDistributedSQLiteCache_ReplacesPreviouslyUserRegisteredServices()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped(typeof(IDistributedCache), sp => Mock.Of<IDistributedCache>());

            // Act
            services.AddDistributedSQLiteCache(options => {
                options.ConnectionString = "Fake";
                options.SchemaName = "Fake";
                options.TableName = "Fake";
            });

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var distributedCache = services.FirstOrDefault(desc => desc.ServiceType == typeof(IDistributedCache));
            Assert.NotNull(distributedCache);
            Assert.Equal(ServiceLifetime.Scoped, distributedCache.Lifetime);
            Assert.IsType<SQLiteCache>(serviceProvider.GetRequiredService<IDistributedCache>());
        }

        [Fact]
        public void AddDistributedSqlServerCache_allows_chaining()
        {
            var services = new ServiceCollection();
            Assert.Same(services, services.AddDistributedSQLiteCache(_ => { }));
        }
    }
}
