using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Mediation.Validation;
using MediatR;

namespace Infrastructure.Mediation.Query
{
    public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, QueryResponse<TResult>>
        where TQuery : IQuery<TResult> { }

    public abstract class QueryHandler<TQuery, TResult> : IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        public abstract Task<QueryResponse<TResult>> Handle(TQuery request, CancellationToken cancellationToken);
        protected static QueryResponse<TResult> Succeeded(TResult result) => QueryResponse.Succeeded(result);
        protected static QueryResponse<TResult> Failed(string message) => QueryResponse.Failed<TResult>(message);
        protected static QueryResponse<TResult> Failed(string key, string message) => QueryResponse.Failed<TResult>(key, message);
        protected static QueryResponse<TResult> Failed(ValidationMessage message) => QueryResponse.Failed<TResult>(message);
        protected static QueryResponse<TResult> Failed(IEnumerable<ValidationMessage> messages) => QueryResponse.Failed<TResult>(messages);
        protected static QueryResponse<TResult> Failed(Exception exception) => QueryResponse.Failed<TResult>(exception);
        protected static QueryResponse<TResult> Failed(Response response) => QueryResponse.Failed<TResult>(response);
    }
}