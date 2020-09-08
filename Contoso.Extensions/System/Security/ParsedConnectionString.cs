using System.Collections.Generic;
using System.Net;

namespace System.Security
{
    /// <summary>
    /// ParsedConnectionString
    /// </summary>
    public class ParsedConnectionString
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParsedConnectionString"/> class.
        /// </summary>
        /// <param name="connectionString">The value.</param>
        /// <exception cref="InvalidOperationException">Unable to read credential store</exception>
        public ParsedConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return;
            string serviceCredential = null, serviceLogin = null, servicePassword = null;
            foreach (var param in connectionString.Split(';'))
            {
                if (string.IsNullOrEmpty(param)) continue;
                var kv = param.Split(new[] { '=' }, 2);
                var key = kv[0]?.Replace(" ", "").ToLowerInvariant();
                if (kv.Length > 1 && key == "credential") serviceCredential = kv[1];
                else if (kv.Length > 1 && (key == "userid" || key == "uid")) serviceLogin = kv[1];
                else if (kv.Length > 1 && (key == "password" || key == "pwd")) servicePassword = kv[1];
                else if (kv.Length > 1 && (key == "server" || key == "datasource")) Server = kv[1];
                else Params.Add(key, kv.Length > 1 ? kv[1] : null);
            }
            if (string.IsNullOrEmpty(serviceCredential)) Credential = new NetworkCredential { UserName = serviceLogin, Password = servicePassword };
            else if (CredentialManager.TryRead(serviceCredential, CredentialManager.CredentialType.GENERIC, out var credential) != 0)
                throw new InvalidOperationException("Unable to read credential store");
            else Credential = new NetworkCredential { UserName = credential.UserName, Password = credential.CredentialBlob };
        }

        /// <summary>
        /// Gets the server.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        public string Server { get; }

        /// <summary>
        /// Gets the credential.
        /// </summary>
        /// <value>
        /// The credential.
        /// </value>
        public NetworkCredential Credential { get; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public Dictionary<string, string> Params { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
