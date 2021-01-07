using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Net;
using System.ServiceModel.Description;

namespace CRM
{
    /// <summary>
    /// ICrmContext
    /// </summary>
    public interface ICrmContext
    {
        /// <summary>
        /// Connects this instance.
        /// </summary>
        /// <returns></returns>
        CrmClient Connect();
    }

    /// <summary>
    /// CrmContext
    /// </summary>
    public class CrmContext : ICrmContext
    {
        readonly Uri _endpointUri;
        readonly Action _attach;
        ClientCredentials _clientCredentials;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrmContext"/> class.
        /// </summary>
        /// <param name="options">The configuration.</param>
        public CrmContext(ICrmOptions options)
            : this(options.Endpoint, options.ServiceLogin) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="CrmContext"/> class.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="credential">The credential.</param>
        /// <exception cref="System.ArgumentNullException">credential</exception>
        /// <exception cref="System.InvalidOperationException">Cannot connect to organization service at {endpoint}</exception>
        CrmContext(string endpoint, NetworkCredential credential)
        {
            if (credential == null)
                throw new ArgumentNullException(nameof(credential));
            if (!Uri.TryCreate(endpoint, UriKind.RelativeOrAbsolute, out _endpointUri))
                throw new InvalidOperationException($"Cannot connect to organization service at {endpoint}");
            _attach = () =>
            {
                var serviceConfig = (IServiceConfiguration<IOrganizationService>)null;
                _clientCredentials = CreateCredentials(serviceConfig, credential);
            };
        }

        /// <summary>
        /// Connects this instance.
        /// </summary>
        /// <returns></returns>
        public CrmClient Connect()
        {
            if (_clientCredentials == null) _attach();
            return new CrmClient(CreateProxy());
        }

        static ClientCredentials CreateCredentials(IServiceConfiguration<IOrganizationService> serviceConfig, NetworkCredential credential)
        {
            var credentials = new ClientCredentials { SupportInteractive = false };
            credentials.UserName.UserName = credential.UserName;
            credentials.UserName.Password = credential.Password;
            return credentials;
            //if (serviceConfig != null && serviceConfig.IssuerEndpoints != null && serviceConfig.IssuerEndpoints.ContainsKey(TokenServiceCredentialType.Username.ToString()))
            //{
            //    credentials.UserName.UserName = string.Empty;
            //    credentials.UserName.Password = string.Empty;
            //    return credentials;
            //}
            //credentials.Windows.ClientCredential = credential;
        }

        OrganizationServiceProxy CreateProxy()
        {
            var proxy = new OrganizationServiceProxy(_endpointUri, null, _clientCredentials, null);
            proxy.EnableProxyTypes();
            proxy.Timeout = new TimeSpan(0, 5, 0);
            return proxy;
        }
    }
}
