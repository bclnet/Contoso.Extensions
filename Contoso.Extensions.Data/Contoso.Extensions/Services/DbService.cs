using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;

namespace Contoso.Extensions.Services
{
    public interface IDbService
    {
        IDbConnection GetConnection(string name = null);
    }

    public class DbService : IDbService
    {
        public static int CommandTimeout => 60;
        public static int LongCommandTimeout => 360;
        public static int VeryLongCommandTimeout => 3600;

        public IDbConnection GetConnection(string name = null)
        {
            var configuration = ConfigBase.Configuration ?? throw new InvalidOperationException("ConfigBase.Configuration must be set before using GetConnection()");
            var conn = new SqlConnection(configuration.GetConnectionString(name ?? "Main"));
            var connString = $";{conn.ConnectionString}".Replace(" ", "").ToLowerInvariant();
            var hasCredential = connString.Contains(";userid=") || connString.Contains(";uid=") || connString.Contains(";password=") || connString.Contains(";pwd=");
            if (!hasCredential && conn.DataSource.EndsWith("database.windows.net", StringComparison.OrdinalIgnoreCase))
                conn.AccessToken = new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/").Result;
            return conn;
        }
    }
}
