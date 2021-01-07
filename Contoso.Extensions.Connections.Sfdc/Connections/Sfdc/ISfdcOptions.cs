using System.Net;

namespace Contoso.Extensions.Connections.Sfdc
{
    /// <summary>
    /// ISfdcOptions
    /// </summary>
    public interface ISfdcOptions
    {
        string Endpoint { get; }
        NetworkCredential ServiceLogin { get; }
        string OrganizationId { get; }
    }
}
