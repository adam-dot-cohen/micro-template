using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Laso.Mediation.Behaviors
{
    [DebuggerStepThrough]
    public class PerfLoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<Response>
        where TResponse : Response
    {
        private readonly ILogger<PerfLoggingPipelineBehavior<TRequest, TResponse>> _logger;

        public PerfLoggingPipelineBehavior(ILogger<PerfLoggingPipelineBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var operationName = $"Handler<{typeof(TRequest)}, {typeof(TResponse)}>";
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

        private static object GetOperationCompleted(string operationName, Activity stopwatch)
        {
            return new { Name = operationName, Status = "Completed", stopwatch.Duration.TotalSeconds };
        }
    }
}
