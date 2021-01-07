using Microsoft.Xrm.Sdk;

namespace CRM
{
    public class CrmClient : CrmServiceContext
    {
        public CrmClient(IOrganizationService service) : base(service) { }
    }
}
