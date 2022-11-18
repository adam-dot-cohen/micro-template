using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Mediation.Event;
using MediatR;

namespace Infrastructure.Mediation.Behaviors
{
    public class EventPipeline<TEvent, THandler> : INotificationHandler<TEvent>
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>
    {
        private readonly THandler _eventHandler;
        private readonly ICollection<IEventPipelineBehavior<TEvent>> _pipelineBehaviors;

        public EventPipeline(THandler eventHandler, ICollection<IEventPipelineBehavior<TEvent>> pipelineBehaviors)
        {
            _eventHandler = eventHandler;
            _pipelineBehaviors = pipelineBehaviors;
        }

        public async Task Handle(TEvent notification, CancellationToken cancellationToken)
        {
            async Task<EventResponse> Handler() => await _eventHandler.Handle(notification, cancellationToken);

            await _pipelineBehaviors
                .Reverse()
                .Aggregate((RequestHandlerDelegate<EventResponse>) Handler, (next, pipeline) => () => pipeline.Handle(notification, next, cancellationToken))();
        }
    }
}