using System.Threading;
using System.Threading.Tasks;

namespace Laso.AdminPortal.Infrastructure.IntegrationEvents
{
    public interface IEventListener
    {
        public Task Open(CancellationToken stoppingToken);
    }
}