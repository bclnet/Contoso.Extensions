using Contoso.Extensions.Configuration;
using Contoso.Extensions.Data;
using Microsoft.Extensions.Configuration;
using System;

namespace Contoso.Extensions.Services
{
    public interface IEmailService
    {
        IEmailConnection GetConnection(string name = null);
    }

    public class EmailService : IEmailService
    {
        public IEmailConnection GetConnection(string name = null)
        {
            var configuration = ConfigBase.Configuration ?? throw new InvalidOperationException("ConfigBase.Configuration must be set before using GetConnection()");
            return new EmailConnection(configuration.GetConnectionString(name ?? "Email"));
        }
    }
}
