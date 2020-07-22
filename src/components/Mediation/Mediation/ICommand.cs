using MediatR;

namespace Laso.Mediation
{
    public interface ICommand<TResult> : IRequest<Response<TResult>>
    {
    }

    public interface ICommand : IRequest<Response>
    {
    }
}