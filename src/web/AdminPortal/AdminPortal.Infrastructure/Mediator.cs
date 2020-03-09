using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Mediator;

namespace Laso.AdminPortal.Infrastructure
{
    public class Mediator : IMediator
    {
        public Task<CommandResponse> Command(ICommand command, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<QueryResponse<TResult>> Query<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
