using MediatR;

namespace Laso.Mediation
{
    public interface ICommand<TResult> : IRequest<CommandResponse<TResult>>
    {
    }

    public interface ICommand : IRequest<CommandResponse>
    {
    }
}