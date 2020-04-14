using System.Threading;
using System.Threading.Tasks;

namespace Laso.Provisioning.Core
{
    public interface ISubscriptionProvisioningService
    {
        Task ProvisionPartner(string partnerId, string name, CancellationToken cancellationToken);
        Task RemovePartner(string partnerId, CancellationToken cancellationToken);
    }
}
