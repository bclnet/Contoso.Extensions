using Contoso.Extensions.Caching.Stream;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Linq;
using Xunit;

namespace Contoso.Extensions.Caching.FileStream
{
    public class CacheServiceExtensionsTests
    {
        [Fact]
        public void AddFileStreamCache_RegistersStreamCacheAsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddFileStreamCache(options => { });

            // Assert
            var streamCache = services.FirstOrDefault(desc => desc.ServiceType == typeof(IStreamCache));
            Assert.NotNull(streamCache);
            Assert.Equal(ServiceLifetime.Singleton, streamCache.Lifetime);
        }

        [Fact]
        public void AddFileStreamCache_ReplacesPreviouslyUserRegisteredServices()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped(typeof(IStreamCache), sp => Mock.Of<IStreamCache>());

            // Act
            services.AddFileStreamCache(options => { });

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var streamCache = services.FirstOrDefault(desc => desc.ServiceType == typeof(IStreamCache));
            Assert.NotNull(streamCache);
            Assert.Equal(ServiceLifetime.Scoped, streamCache.Lifetime);
            Assert.IsType<FileStreamCache>(serviceProvider.GetRequiredService<IStreamCache>());
        }

        [Fact]
        public void AddFileStreamCache_allows_chaining()
        {
            var services = new ServiceCollection();
            Assert.Same(services, services.AddFileStreamCache(_ => { }));
        }
    }
}
