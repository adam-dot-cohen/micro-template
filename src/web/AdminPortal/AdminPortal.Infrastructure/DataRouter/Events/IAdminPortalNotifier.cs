using System.Threading;
using System.Threading.Tasks;

namespace Laso.AdminPortal.Infrastructure.DataRouter.Events
{
    public interface IAdminPortalNotifier
    {
        Task Notify(string message, CancellationToken cancellationToken);
    }
}
