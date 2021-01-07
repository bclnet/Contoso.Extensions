using System.Net;

namespace CRM
{
    public interface ICrmOptions
    {
        string Endpoint { get; }
        NetworkCredential ServiceLogin { get; }
    }
}
