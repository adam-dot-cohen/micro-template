using System.Threading.Tasks;

namespace Laso.Provisioning.Core
{
    public interface IEventPublisher
    {
        Task Publish(object @event);
    }
}
