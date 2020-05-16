using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Linq;
using Xunit;

namespace Contoso.Extensions.Caching.FileSystem
{
    public class CacheServiceExtensionsTests
    {
        [Fact]
        public void AddFileSystemCache_RegistersDistributedCacheAsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddFileSystemCache(options => { });

            // Assert
            var distributedCache = services.FirstOrDefault(desc => desc.ServiceType == typeof(IDistributedCache));
            Assert.NotNull(distributedCache);
            Assert.Equal(ServiceLifetime.Singleton, distributedCache.Lifetime);
        }

        [Fact]
        public void AddFileSystemCache_ReplacesPreviouslyUserRegisteredServices()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped(typeof(IDistributedCache), sp => Mock.Of<IDistributedCache>());

            // Act
            services.AddFileSystemCache(options => { });

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var distributedCache = services.FirstOrDefault(desc => desc.ServiceType == typeof(IDistributedCache));
            Assert.NotNull(distributedCache);
            Assert.Equal(ServiceLifetime.Scoped, distributedCache.Lifetime);
            Assert.IsType<FileSystemCache>(serviceProvider.GetRequiredService<IDistributedCache>());
        }

        [Fact]
        public void AddFileSystemCache_allows_chaining()
        {
            var services = new ServiceCollection();
            Assert.Same(services, services.AddFileSystemCache(_ => { }));
        }
    }
}
