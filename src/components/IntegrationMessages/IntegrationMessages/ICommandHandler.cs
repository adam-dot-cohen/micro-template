using System.Threading;
using System.Threading.Tasks;

namespace Laso.IntegrationMessages
{
    public interface ICommandHandler<in T> where T : IIntegrationMessage
    {
        Task Handle(T command, CancellationToken cancellationToken);
    }
}