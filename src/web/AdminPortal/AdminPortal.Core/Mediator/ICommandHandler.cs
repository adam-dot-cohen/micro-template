using System.Threading;
using System.Threading.Tasks;

namespace Laso.AdminPortal.Core.Mediator
{
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        Task<CommandResponse> Handle(TCommand query, CancellationToken cancellationToken);
    }
}
