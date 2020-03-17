using System.Threading.Tasks;

namespace Laso.AdminPortal.Core.IntegrationEvents
{
    public interface IEventSender
    {
        Task Send<T>(T @event) where T : IIntegrationEvent;
    }
}
