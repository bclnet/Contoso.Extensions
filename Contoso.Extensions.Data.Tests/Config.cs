using Microsoft.Extensions.Configuration;
using Moq;

namespace Contoso.Extensions
{
    internal class Config : ConfigBase
    {
        static Config() => Configure();

        public static void Setup()
        {
            var confSectionMock = new Mock<IConfigurationSection>();
            confSectionMock.SetupGet(m => m[It.Is<string>(s => s == "Main")]).Returns(MockConnectionString);
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(a => a.GetSection(It.Is<string>(s => s == "ConnectionStrings"))).Returns(confSectionMock.Object);
            Configuration = configurationMock.Object;
        }

        public const string AppName = "Config";
        public const string MockConnectionString = "Server=tcp:darwinl-db.database.windows.net;Initial Catalog=DARWINL_Unanet;MultipleActiveResultSets=True;Enlist=False;Encrypt=True;TrustServerCertificate=False;";
    }
}
