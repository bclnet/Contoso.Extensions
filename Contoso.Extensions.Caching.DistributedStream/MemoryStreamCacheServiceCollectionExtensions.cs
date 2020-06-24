using Contoso.Extensions.Caching.MemoryStream;
using Contoso.Extensions.Caching.Stream;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up MemoryStream caching related services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class MemoryStreamCacheServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the MemoryStream caching services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddMemoryStreamCache(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddOptions();
            services.TryAdd(ServiceDescriptor.Singleton<IStreamCache, MemoryStreamCache>());

            return services;
        }

        /// <summary>
        /// Adds the MemoryStream caching services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="setupAction">An <see cref="Action{FileStreamCacheOptions}"/> to configure the provided <see cref="MemoryStreamCacheOptions"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddMemoryStreamCache(this IServiceCollection services, Action<MemoryStreamCacheOptions> setupAction)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (setupAction == null)
                throw new ArgumentNullException(nameof(setupAction));

            services.AddMemoryStreamCache();
            services.Configure(setupAction);

            return services;
        }
    }
}