using System.Threading.Tasks;

namespace Laso.Identity.Core.IntegrationEvents
{
    public interface IEventPublisher
    {
        Task Publish<T>(T @event) where T : IIntegrationEvent;
    }

    public interface IIntegrationEvent { }

    public interface IEnvelopedIntegrationEvent : IIntegrationEvent
    {
        (string Name, object Value) Discriminator { get; }
    }
}
