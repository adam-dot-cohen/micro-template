using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Laso.Mediation.Behaviors
{
    [DebuggerStepThrough]
    public class ErrorLoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<Response>
        where TResponse : Response
    {
        private readonly ILogger<ErrorLoggingPipelineBehavior<TRequest, TResponse>> _logger;

        public ErrorLoggingPipelineBehavior(ILogger<ErrorLoggingPipelineBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
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
                MessageType = (typeof(TRequest)).Name,
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
