using MediatR;

namespace Laso.Mediation
{
    public interface IQuery<TResult> : IRequest<QueryResponse<TResult>>
    {
    }
}