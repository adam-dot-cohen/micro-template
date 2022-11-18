using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Mediation.Event;
using Infrastructure.Mediation.Validation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Mediation.Behaviors
{
    public class ErrorLoggingPipelineBehavior<TRequest, TResponse> :
        ErrorLoggingPipelineBehaviorBase<TRequest, TResponse>, IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : Response
    {
        public ErrorLoggingPipelineBehavior(ILogger<ErrorLoggingPipelineBehaviorBase<TRequest, TResponse>> logger) : base(logger) { }

    }

    public class ErrorLoggingEventPipelineBehavior<TEvent> : ErrorLoggingPipelineBehaviorBase<TEvent, EventResponse>, IEventPipelineBehavior<TEvent>
        where TEvent : IEvent
    {
        public ErrorLoggingEventPipelineBehavior(ILogger<ErrorLoggingPipelineBehaviorBase<TEvent, EventResponse>> logger) : base(logger) { }
        
    }

    [DebuggerStepThrough]
    public abstract class ErrorLoggingPipelineBehaviorBase<TRequest, TResponse>
        where TResponse : Response
    {
        private readonly ILogger _logger;

        protected ErrorLoggingPipelineBehaviorBase(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            TResponse response;
            try
            {
                response = await next().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                LogError(e);
                throw;
            }

            if (!response.Success)
            {
                LogError(response.Exception, response.ValidationMessages);
            }

            return response;
        }

        private void LogError(Exception exception, IEnumerable<ValidationMessage> validationMessages = null)
        {
            var errorContext = new
            {
                MessageType = typeof(TRequest).Name,
                ValidationMessages = validationMessages ?? new ValidationMessage[0]
            };
            if (exception == null)
            {
                _logger.LogWarning("Handler Error: {@HandlerResultDetails}", errorContext);
            }
            else
            {
                _logger.LogError(exception, "Handler Error: {@HandlerResultDetails}", errorContext);
            }
        }
    }
}
