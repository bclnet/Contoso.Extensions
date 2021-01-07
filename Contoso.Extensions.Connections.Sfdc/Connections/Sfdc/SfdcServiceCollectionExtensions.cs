using Contoso.Extensions.Connections.Sfdc;
using System;
using System.Net;
using System.Security;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// SfdcServiceCollectionExtensions
    /// </summary>
    public static class SfdcServiceCollectionExtensions
    {
        class ParsedSfdcOptions : ISfdcOptions
        {
            readonly ParsedConnectionString _parsedConnectionString;
            public ParsedSfdcOptions(string connectionString) => _parsedConnectionString = new ParsedConnectionString(connectionString);
            public string Endpoint => $"https://{_parsedConnectionString.Server}";
            public NetworkCredential ServiceLogin => _parsedConnectionString.Credential;
            public string OrganizationId => _parsedConnectionString.Params.TryGetValue("orgid", out var z) ? z : null;
        }

        /// <summary>
        /// Adds the SFDC context.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">services</exception>
        public static IServiceCollection AddSfdcContext(this IServiceCollection services, ISfdcConnectionString config, string name = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            services.Add(ServiceDescriptor.Singleton<ISfdcContext>(new SfdcContext(new ParsedSfdcOptions(config[name]))));
            return services;
        }
        /// <summary>
        /// Adds the SFDC context.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="options">The configuration.</param>
        /// <returns>IServiceCollection.</returns>
        /// <exception cref="ArgumentNullException">services</exception>
        public static IServiceCollection AddSfdcContext(this IServiceCollection services, ISfdcOptions options)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            services.Add(ServiceDescriptor.Singleton<ISfdcContext>(new SfdcContext(options)));
            return services;
        }
    }
}
