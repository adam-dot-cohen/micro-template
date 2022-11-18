using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Mediation.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Mediation.Behaviors
{
    public class PerfLoggingPipelineBehavior<TRequest, TResponse> : PerfLoggingPipelineBehaviorBase<TRequest, TResponse>, IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : Response
    {
        public PerfLoggingPipelineBehavior(ILogger<PerfLoggingPipelineBehavior<TRequest, TResponse>> logger) : base(logger) { }
    }

    public class PerfLoggingEventPipelineBehavior<TEvent> : PerfLoggingPipelineBehaviorBase<TEvent, EventResponse>, IEventPipelineBehavior<TEvent>
        where TEvent : IEvent
    {
        public PerfLoggingEventPipelineBehavior(ILogger<PerfLoggingEventPipelineBehavior<TEvent>> logger) : base(logger) { }
    }

    [DebuggerStepThrough]
    public abstract class PerfLoggingPipelineBehaviorBase<TRequest, TResponse>
    {
        private readonly ILogger _logger;

        protected PerfLoggingPipelineBehaviorBase(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var operationName = $"Handler<{typeof(TRequest)}, {typeof(TResponse)}>";

            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            var activity = new Activity(operationName);

            _logger.LogDebug("Handling {@Operation}", GetOperationStarted(operationName));
            TResponse response;
            try
            {
                activity.Start();
                response = await next().ConfigureAwait(false);
            }
            finally
            {
                activity.Stop();
                _logger.LogInformation("Handled {@Operation}", GetOperationCompleted(operationName, activity));
            }

            return response;
        }

        private static object GetOperationStarted(string operationName)
        {
            return new { Name = operationName, Status = "Started" };
        }

        private static object GetOperationCompleted(string operationName, Activity activity)
        {
            return new { Name = operationName, Status = "Completed", activity.Duration.TotalSeconds };
        }
    }
}
