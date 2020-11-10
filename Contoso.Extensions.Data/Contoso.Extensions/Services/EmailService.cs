using Contoso.Extensions.Data;
using Microsoft.Extensions.Configuration;
using System;

namespace Contoso.Extensions.Services
{
    /// <summary>
    /// IEmailService
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        IEmailConnection GetConnection(string name = null);
    }

    /// <summary>
    /// EmailService
    /// </summary>
    /// <seealso cref="Contoso.Extensions.Services.IEmailService" />
    public class EmailService : IEmailService
    {
        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">ConfigBase.Configuration must be set before using GetConnection()</exception>
        public IEmailConnection GetConnection(string name = null)
        {
            var configuration = ConfigBase.Configuration ?? throw new InvalidOperationException("ConfigBase.Configuration must be set before using GetConnection()");
            return new EmailConnection(configuration.GetConnectionString(name ?? "Email"));
        }
    }
}
