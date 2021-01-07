using CRM;
using System;
using System.Net;
using System.Security;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// CrmServiceCollectionExtensions
    /// </summary>
    public static class CrmServiceCollectionExtensions
    {
        class ParsedCrmOptions : ICrmOptions
        {
            readonly ParsedConnectionString _parsedConnectionString;
            public ParsedCrmOptions(string connectionString) => _parsedConnectionString = new ParsedConnectionString(connectionString);
            public string Endpoint => $"https://{_parsedConnectionString.Server}";
            public NetworkCredential ServiceLogin => _parsedConnectionString.Credential;
        }

        /// <summary>
        /// Adds the CRM backend.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="name">The name.</param>
        /// <returns>IServiceCollection.</returns>
        /// <exception cref="ArgumentNullException">services</exception>
        public static IServiceCollection AddCrmContext(this IServiceCollection services, ICrmConnectionString config, string name = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            services.Add(ServiceDescriptor.Singleton<ICrmContext>(new CrmContext(new ParsedCrmOptions(config[name]))));
            return services;
        }
        /// <summary>
        /// Adds the CRM backend.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="options">The configuration.</param>
        /// <returns>IServiceCollection.</returns>
        /// <exception cref="ArgumentNullException">services</exception>
        public static IServiceCollection AddCrmContext(this IServiceCollection services, ICrmOptions options)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            services.Add(ServiceDescriptor.Singleton<ICrmContext>(new CrmContext(options)));
            return services;
        }
    }
}
