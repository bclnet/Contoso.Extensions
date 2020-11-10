using Contoso.Extensions.Caching.Stream;
using Contoso.Extensions.Caching.FileStream;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up FileStream caching related services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class FileStreamCacheServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the FileStream caching services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="setupAction">An <see cref="Action{FileStreamCacheOptions}"/> to configure the provided <see cref="FileStreamCacheOptions"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddFileStreamCache(this IServiceCollection services, Action<FileStreamCacheOptions> setupAction)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (setupAction == null)
                throw new ArgumentNullException(nameof(setupAction));

            services.AddOptions();
            services.Configure(setupAction);
            services.Add(ServiceDescriptor.Singleton<IStreamCache, FileStreamCache>());

            return services;
        }
    }
}