using System;
using System.Threading.Tasks;

namespace Laso.Provisioning.Core
{
    public interface ISubscriptionProvisioningService
    {
        void ProvisionNewPartner(string partnerId);
    }
}
