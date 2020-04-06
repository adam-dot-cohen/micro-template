using MediatR;

namespace Laso.Identity.Core.Mediator
{
    public interface ICommand<TResult> : IRequest<Response<TResult>>
    {
    }

    public interface ICommand : IRequest<Response>
    {
    }
}