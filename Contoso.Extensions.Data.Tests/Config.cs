using Microsoft.Extensions.Configuration;
using Moq;
using System;

namespace Contoso.Extensions
{
    internal class Config : ConfigBase
    {
        static Config() => Configure();
        public const string AppName = "Test";

        public static void Setup()
        {
            var connectionString = Environment.GetEnvironmentVariable("BCLNET_CONNSTRING") ?? throw new Exception("Please Set BCLNET_CONNSTRING Environment Variable");
            var confSectionMock = new Mock<IConfigurationSection>();
            confSectionMock.SetupGet(m => m[It.Is<string>(s => s == "Main")]).Returns(connectionString);
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(a => a.GetSection(It.Is<string>(s => s == "ConnectionStrings"))).Returns(confSectionMock.Object);
            Configuration = configurationMock.Object;
        }
    }
}
