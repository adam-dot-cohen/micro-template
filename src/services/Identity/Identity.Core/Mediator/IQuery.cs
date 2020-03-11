using MediatR;

namespace Laso.Identity.Core.Mediator
{
    public interface IQuery<TResult> : IRequest<Response<TResult>>
    {
    }
}