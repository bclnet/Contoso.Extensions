using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;

namespace Contoso.Extensions.Services
{
    public interface IDbService
    {
        IDbConnection GetConnection(string name = null, bool skipAzure = false);
        string GetConnectionString(string name = null, bool skipAzure = false);
    }

    public class DbService : IDbService
    {
        public static int CommandTimeout => 60;
        public static int LongCommandTimeout => 360;
        public static int VeryLongCommandTimeout => 3600;

        public IDbConnection GetConnection(string name = null, bool skipAzure = false)
        {
            var configuration = ConfigBase.Configuration ?? throw new InvalidOperationException("ConfigBase.Configuration must be set before using GetConnection()");
            var conn = new SqlConnection(configuration.GetConnectionString(name ?? "Main"));
            if (skipAzure)
                return conn;
            var connSearch = $";{conn.ConnectionString}".Replace(" ", "").ToLowerInvariant();
            var hasCredential = connSearch.Contains(";userid=") || connSearch.Contains(";uid=") || connSearch.Contains(";password=") || connSearch.Contains(";pwd=");
            if (!hasCredential && conn.DataSource.EndsWith("database.windows.net", StringComparison.OrdinalIgnoreCase))
                conn.AccessToken = new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/").Result;
            return conn;
        }

        public string GetConnectionString(string name = null, bool skipAzure = false)
        {
            var configuration = ConfigBase.Configuration ?? throw new InvalidOperationException("ConfigBase.Configuration must be set before using GetConnection()");
            var connString = configuration.GetConnectionString(name ?? "Main");
            if (skipAzure)
                return connString;
            var connSearch = $";{connString}".Replace(" ", "").ToLowerInvariant();
            var hasCredential = connSearch.Contains(";userid=") || connSearch.Contains(";uid=") || connSearch.Contains(";password=") || connSearch.Contains(";pwd=");
            return !hasCredential && connSearch.Contains("database.windows.net")
                ? $"{connString};Access Token={new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/").Result}"
                : connString;
        }
    }
}
