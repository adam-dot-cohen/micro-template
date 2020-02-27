using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Laso.AdminPortal.Web.Hubs
{
    public class NotificationsHub : Hub
    {
        // TODO: Consider making notification -> hub integration part of mediation. [jay_mclain]
        public Task Send(string data)
        {
            return Clients.All.SendAsync("Notify", data);
        }
    }
}
