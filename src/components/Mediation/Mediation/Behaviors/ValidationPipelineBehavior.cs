using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Mediation.Validation;
using MediatR;

namespace Infrastructure.Mediation.Behaviors
{
    [DebuggerStepThrough]
    public class ValidationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : Response, new()
    {
 
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is IInputValidator validator)
            {
                var result = validator.ValidateInput();
                if (!result.Success)
                {
                    return result.ToResponse<TResponse>();
                }
            }

            return await next().ConfigureAwait(false);
        }
    }
}