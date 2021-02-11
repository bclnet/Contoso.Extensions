using Microsoft.Extensions.Configuration;
using System;

namespace Contoso.Extensions
{
    /// <summary>
    /// ConfigBase
    /// </summary>
    public abstract class ConfigBase
    {
        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public static IConfiguration Configuration { get; set; }

        /// <summary>
        /// Sets the configuration using a polyfill.
        /// </summary>
        /// <param name="configure">The configure.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="environmentName">Name of the environment.</param>
        public static void Configure(Action<IConfigurationBuilder> configure = null, string prefix = null, string environmentName = null)
        {
            if (string.IsNullOrEmpty(environmentName))
                environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true);
            builder = string.IsNullOrEmpty(prefix)
                ? builder.AddEnvironmentVariables()
                : builder.AddEnvironmentVariables(prefix);
            configure?.Invoke(builder);
            Configuration = builder.Build();
        }
    }
}
