using MediatR;

namespace Infrastructure.Mediation.Query
{
    public interface IQuery<TResult> : IRequest<QueryResponse<TResult>> { }
}