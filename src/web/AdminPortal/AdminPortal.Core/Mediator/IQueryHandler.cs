using System.Threading;
using System.Threading.Tasks;

namespace Laso.AdminPortal.Core.Mediator
{
    public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
    {
        Task<QueryResponse<TResult>> Handle(TQuery query, CancellationToken cancellationToken);
    }
}
