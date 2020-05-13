using Microsoft.Extensions.Configuration;

namespace Contoso.Extensions.Configuration
{
    public abstract class ConfigBase
    {
        public static IConfiguration Configuration { get; set; }

        public static void LegacyDotNet(string app = "APP")
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables(prefix: $"{app}_")
                .Build();
        }
    }
}
