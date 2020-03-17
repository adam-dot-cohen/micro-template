using System.Threading;
using System.Threading.Tasks;

namespace Laso.AdminPortal.Core.Mediator
{
    public interface ICommandHandler<in TCommand>
        where TCommand : ICommand
    {
        Task<CommandResponse> Handle(TCommand query, CancellationToken cancellationToken);
    }

    public interface ICommandHandler<in TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        Task<CommandResponse<TResult>> Handle(TCommand query, CancellationToken cancellationToken);
    }
}
