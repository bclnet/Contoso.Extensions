using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Contoso.Extensions.Connections.Sfdc
{
    /// <summary>
    /// ISfdcContext
    /// </summary>
    public interface ISfdcContext
    {
        /// <summary>
        /// Connects the specified subsite.
        /// </summary>
        /// <param name="subsite">The subsite.</param>
        /// <returns></returns>
        SfdcClient Connect(string subsite = null);
    }

    /// <summary>
    /// SfdcContext
    /// </summary>
    public class SfdcContext : ISfdcContext
    {
        static readonly Binding _binding = GetSoapBinding();
        readonly Action _attach;
        EndpointAddress _endpoint;
        SessionHeader _header;

        /// <summary>
        /// Initializes a new instance of the <see cref="SfdcContext"/> class.
        /// </summary>
        /// <param name="options">The configuration.</param>
        public SfdcContext(ISfdcOptions options)
            : this(options.Endpoint, options.ServiceLogin, options.OrganizationId) { }
        SfdcContext(string endpoint, NetworkCredential credential, string organizationId)
        {
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));
            if (credential == null)
                throw new ArgumentNullException(nameof(credential));
            _attach = () =>
            {
                var soapClient = new SoapClient(GetSoapBinding(), new EndpointAddress(new Uri(endpoint))); // Create the SOAP binding for call with Oauth.
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using (var scope = new OperationContextScope(soapClient.InnerChannel))
                {
                    var result = soapClient.login(string.IsNullOrEmpty(organizationId) ? null : new LoginScopeHeader { organizationId = organizationId }, credential.UserName, credential.Password);
                    _endpoint = new EndpointAddress(result.serverUrl);
                    _header = new SessionHeader { sessionId = result.sessionId };
                }
            };
        }

        /// <summary>
        /// Connects the specified subsite.
        /// </summary>
        /// <param name="subsite">The subsite.</param>
        /// <returns>SfdcContext.</returns>
        public SfdcClient Connect(string subsite = null)
        {
            if (_endpoint == null) _attach();
            return new SfdcClient { Client = new SoapClient(_binding, _endpoint), Header = _header };
        }

        static Binding GetSoapBinding()
        {
            var binding = new BasicHttpBinding
            {
                Name = "UserNameSoapBinding",
                MaxReceivedMessageSize = 2147483647,
            };
            binding.Security.Mode = BasicHttpSecurityMode.Transport;
            return binding;
        }
    }
}
