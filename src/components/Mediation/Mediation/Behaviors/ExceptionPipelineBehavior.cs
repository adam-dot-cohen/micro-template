using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Mediation.Event;
using MediatR;

namespace Infrastructure.Mediation.Behaviors
{
    public class ExceptionPipelineBehavior<TRequest, TResponse> : ExceptionPipelineBehaviorBase<TRequest, TResponse>, IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : Response, new()
    { }

    public class ExceptionEventPipelineBehavior<TEvent> : ExceptionPipelineBehaviorBase<TEvent, EventResponse>, IEventPipelineBehavior<TEvent>
        where TEvent : IEvent
    { }

    [DebuggerStepThrough]
    public abstract class ExceptionPipelineBehaviorBase<TRequest, TResponse>
        where TResponse : Response, new()
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var response = new TResponse
                {
                    Exception = e
                };

                return response;
            }
        }
    }
}