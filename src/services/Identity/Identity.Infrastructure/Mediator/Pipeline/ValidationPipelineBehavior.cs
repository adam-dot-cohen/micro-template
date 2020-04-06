﻿using System.Threading;
using System.Threading.Tasks;
using Laso.Identity.Core.Mediator;
using MediatR;

namespace Laso.Identity.Infrastructure.Mediator.Pipeline
{
    public class ValidationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<Response>, IInputValidator
        where TResponse : Response, new()
    {
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            if (request is IInputValidator validator)
            {
                var result = validator.ValidateInput();
                if (!result.IsValid)
                {
                    return result.ToResponse<TResponse>();
                }
            }

            return await next();
        }
    }
}