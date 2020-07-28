using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Laso.Mediation.Behaviors
{
    [DebuggerStepThrough]
    public class ValidationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        // IInputValidator constraint ensures behavior is only invoked for requests supporting validation
        where TRequest : IRequest<Response>, IInputValidator
        where TResponse : Response, new()
    {
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var result = request.ValidateInput();
            if (!result.Success)
            {
                return result.ToResponse<TResponse>();
            }

            return await next().ConfigureAwait(false);
        }
    }
}