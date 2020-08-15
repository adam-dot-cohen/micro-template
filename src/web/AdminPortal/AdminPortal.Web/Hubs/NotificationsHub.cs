using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Infrastructure.DataRouter.Events;
using Microsoft.AspNetCore.SignalR;

namespace Laso.AdminPortal.Web.Hubs
{
    public class NotificationsHub : Hub
    {
        // TODO: Consider making notification -> hub integration part of mediation. [jay_mclain]
        // Is this method used?
        public Task Send(string data)
        {
            return Clients.All.SendAsync("Notify", data);
        }
    }

    public class AdminPortalNotifier : IAdminPortalNotifier
    {
        private readonly IHubContext<NotificationsHub> _hubContext;

        public AdminPortalNotifier(IHubContext<NotificationsHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task Notify(string message, CancellationToken cancellationToken)
        {
            await _hubContext.Clients.All.SendAsync("Notify", "Partner provisioning complete!", cancellationToken);
        }
    }
}
