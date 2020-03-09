using System.Threading.Tasks;
using Laso.AdminPortal.Web.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Laso.AdminPortal.Web.Api.Notifications
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly IHubContext<NotificationsHub> _hubContext;

        public NotificationsController(IHubContext<NotificationsHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Post(string message)
        {
            await _hubContext.Clients.All.SendAsync("Notify", message);
            return Ok();
        }
    }
}
