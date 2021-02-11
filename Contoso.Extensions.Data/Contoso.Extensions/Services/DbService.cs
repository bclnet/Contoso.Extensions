using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;

namespace Contoso.Extensions.Services
{
    /// <summary>
    /// IDbService
    /// </summary>
    public interface IDbService
    {
        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="skipAzure">if set to <c>true</c> [skip azure].</param>
        /// <returns></returns>
        IDbConnection GetConnection(string name = null, bool skipAzure = false);
        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="skipAzure">if set to <c>true</c> [skip azure].</param>
        /// <returns></returns>
        string GetConnectionString(string name = null, bool skipAzure = false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Contoso.Extensions.Services.IDbService" />
    public class DbService : IDbService
    {
        /// <summary>
        /// Default command timeout.
        /// </summary>
        /// <value>
        /// The command timeout.
        /// </value>
        public static int CommandTimeout => 60;
        /// <summary>
        /// A long command timeout.
        /// </summary>
        /// <value>
        /// The long command timeout.
        /// </value>
        public static int LongCommandTimeout => 360;
        /// <summary>
        /// A very long command timeout.
        /// </summary>
        /// <value>
        /// The very long command timeout.
        /// </value>
        public static int VeryLongCommandTimeout => 3600;

        /// <summary>
        /// Gets the connection, adding an Access Token if DataSource is Azure.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="skipAzure">if set to <c>true</c> [skip azure].</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">ConfigBase.Configuration must be set before using GetConnection()</exception>
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

        /// <summary>
        /// Gets the connection string, adding an Access Token if DataSource is Azure.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="skipAzure">if set to <c>true</c> [skip azure].</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">ConfigBase.Configuration must be set before using GetConnection()</exception>
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
