using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Mediation.Event;
using MediatR;

namespace Infrastructure.Mediation.Behaviors
{
    public interface IEventPipelineBehavior<in TEvent> where TEvent : IEvent
    {
        Task<EventResponse> Handle(TEvent notification, RequestHandlerDelegate<EventResponse> next, CancellationToken cancellationToken);
    }
}