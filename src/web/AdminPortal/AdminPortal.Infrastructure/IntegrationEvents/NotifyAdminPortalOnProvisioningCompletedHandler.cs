using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Mediation.Event;
using Laso.AdminPortal.Core.IntegrationEvents;
using Laso.AdminPortal.Infrastructure.SignalR;

namespace Laso.AdminPortal.Infrastructure.IntegrationEvents
{
    public class NotifyAdminPortalOnProvisioningCompletedHandler : IEventHandler<ProvisioningCompletedEvent>
    {
        private readonly IAdminPortalNotifier _adminPortalNotifier;

        public NotifyAdminPortalOnProvisioningCompletedHandler(IAdminPortalNotifier adminPortalNotifier)
        {
            _adminPortalNotifier = adminPortalNotifier;
        }

        public async Task<EventResponse> Handle(ProvisioningCompletedEvent notification, CancellationToken cancellationToken)
        {
            await _adminPortalNotifier.Notify("Partner provisioning complete!", cancellationToken);

            return EventResponse.Succeeded();
        }
    }
}