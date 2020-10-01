using Microsoft.Extensions.Configuration;
using Renci.SshNet;
using System;
using System.IO;
using System.Net;
using System.Security;

namespace Contoso.Extensions.Services
{
    public interface ISshService
    {
        ScpClient GetConnection(string name);
    }

    public class SshService : ISshService
    {
        public ScpClient GetConnection(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            var configuration = ConfigBase.Configuration ?? throw new InvalidOperationException("ConfigBase.Configuration must be set before using GetConnection()");
            var (server, credential, filePath) = ParseConnection(configuration.GetConnectionString(name));
            return GetConnection(server, credential, filePath);
        }

        public static ScpClient GetConnection(string server, NetworkCredential credential, string filePath)
        {
            if (string.IsNullOrEmpty(server))
                throw new ArgumentNullException(nameof(server));
            if (credential == null)
                throw new ArgumentNullException(nameof(credential));
            var fileName = string.IsNullOrEmpty(filePath) ? filePath : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh/id_rsa");
            return new ScpClient(server, credential.UserName, new PrivateKeyFile(fileName, credential.Password));
        }

        public static (string server, NetworkCredential credential, string filePath) ParseConnection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));
            string serviceCredential = null, serviceLogin = null, servicePassword = null, server = null, filePath = null;
            foreach (var param in connectionString.Split(';'))
            {
                if (string.IsNullOrEmpty(param)) continue;
                var kv = param.Split(new[] { '=' }, 2);
                var key = kv[0]?.Replace(" ", "").ToLowerInvariant();
                if (kv.Length > 1 && key == "credential") serviceCredential = kv[1];
                else if (kv.Length > 1 && (key == "userid" || key == "uid")) serviceLogin = kv[1];
                else if (kv.Length > 1 && (key == "password" || key == "pwd")) servicePassword = kv[1];
                else if (kv.Length > 1 && (key == "server" || key == "datasource")) server = kv[1];
                else if (kv.Length > 1 && key == "filepath") filePath = kv[1];
            }
            var credential = string.IsNullOrEmpty(serviceCredential) ? new NetworkCredential { UserName = serviceLogin, Password = servicePassword } :
                CredentialManager.TryRead(serviceCredential, CredentialManager.CredentialType.GENERIC, out var cred) != 0 ? throw new InvalidOperationException("Unable to read credential store") :
                new NetworkCredential { UserName = cred.UserName, Password = cred.CredentialBlob };
            return (server, credential, filePath);
        }
    }
}
