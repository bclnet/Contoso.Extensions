using Contoso.Extensions.Configuration;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;

namespace Contoso.Extensions.Services
{
    public interface IDbService
    {
        IDbConnection GetConnection(string id = null);
        IDbConnection GetAzureConnection(string id = null);
    }

    public class DbService : IDbService
    {
        public static int CommandTimeout => 60;
        public static int LongCommandTimeout => 360;
        public static int VeryLongCommandTimeout => 3600;

        public IDbConnection GetConnection(string name = null)
        {
            var configuration = ConfigBase.Configuration ?? throw new InvalidOperationException("DbService.Configuration must be set before using GetConnection()");
            return new SqlConnection(configuration.GetConnectionString(name ?? "Main"));
        }

        public IDbConnection GetAzureConnection(string name = null)
        {
            var configuration = ConfigBase.Configuration ?? throw new InvalidOperationException("DbService.Configuration must be set before using GetConnection()");
            return new SqlConnection(configuration.GetConnectionString(name ?? "Main"))
            {
                AccessToken = new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/").Result
            };
        }
    }
}
