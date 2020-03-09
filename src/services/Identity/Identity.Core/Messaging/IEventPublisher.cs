using System.Threading.Tasks;

namespace Laso.Identity.Core.Messaging
{
    public interface IEventPublisher
    {
        Task Publish(object @event);
    }
}
