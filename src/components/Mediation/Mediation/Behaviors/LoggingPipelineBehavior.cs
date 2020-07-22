using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Laso.Mediation.Behaviors
{
    public class LoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<Response>
        where TResponse : Response
    {
        private readonly ILogger<LoggingPipelineBehavior<TRequest, TResponse>> _logger;

        public LoggingPipelineBehavior(ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            // TODO: Move non-error handling to perf logging behavior with timing
            _logger.LogDebug($"Handling {typeof(TRequest).Name}");
            TResponse response;
            try
            {
                response = await next();
                _logger.LogInformation($"Handled {typeof(TRequest).Name}");
            }
            catch (Exception e)
            {
                LogError(typeof(TRequest), e);
                throw;
            }

            if (!response.Success())
            {
                LogError(typeof(TRequest), response.Exception, response.ValidationMessages);
            }

            return response;
        }

        private void LogError(Type messageType, Exception exception, IEnumerable<ValidationMessage> validationMessages = null)
        {
            var errorContext = new
            {
                MessageType = messageType.Name,
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