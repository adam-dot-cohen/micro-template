using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.Identity.Core.Mediator;
using MediatR;

namespace Laso.Identity.Infrastructure.Mediator.Pipeline
{
    public class ExceptionPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<Response>
        where TResponse : Response, new()
    {
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            try
            {
                return await next();
            }
            catch (Exception e)
            {
                var response = new TResponse
                {
                    IsValid = false,
                    Exception = e
                };

                return response;
            }
        }
    }
}