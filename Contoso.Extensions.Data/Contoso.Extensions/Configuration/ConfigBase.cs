using Microsoft.Extensions.Configuration;
using System;

namespace Contoso.Extensions.Configuration
{
    public abstract class ConfigBase
    {
        public static IConfiguration Configuration { get; set; }

        public static void LegacyDotNet(string app = "APP", string environmentName = null)
        {
            if (string.IsNullOrEmpty(environmentName))
                environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .AddEnvironmentVariables(prefix: $"{app}_")
                .Build();
        }
    }
}
