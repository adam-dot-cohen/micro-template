using System;
using System.Collections.Generic;

namespace Laso.Mediation
{
    public abstract class QueryResponse : Response
    {
        protected QueryResponse(IEnumerable<ValidationMessage> failures = null, Exception exception = null) : base(failures, exception) { }

        public static QueryResponse<TResult> Succeeded<TResult>(TResult result) => new QueryResponse<TResult>(result: result);

        public static QueryResponse<TResult> Failed<TResult>(string message) => new QueryResponse<TResult>(new[] {new ValidationMessage(string.Empty, message)});
        public static QueryResponse<TResult> Failed<TResult>(string key, string message) => new QueryResponse<TResult>(new[] {new ValidationMessage(key, message)});
        public static QueryResponse<TResult> Failed<TResult>(ValidationMessage message) => new QueryResponse<TResult>(new [] {message});
        public static QueryResponse<TResult> Failed<TResult>(IEnumerable<ValidationMessage> messages) => new QueryResponse<TResult>(messages);
        public static QueryResponse<TResult> Failed<TResult>(Exception exception) => new QueryResponse<TResult>(exception: exception);
        public static QueryResponse<TResult> Failed<TResult>(Response response)
        {
            if (response.Success)
            {
                throw new Exception($"Expected failure response of type: {response.GetType().Name}");
            }

            return response.ToResponse<QueryResponse<TResult>>();
        }

        public static QueryResponse<TResult> From<TResult>(params Response[] responses)
        {
            return Response.From<QueryResponse<TResult>>(responses);
        }
    }

    public class QueryResponse<TResult> : QueryResponse
    {
        public QueryResponse() { }

        public QueryResponse(IEnumerable<ValidationMessage> failures = null, Exception exception = null, TResult result = default) : base(failures, exception)
        {
            Result = result;
        }

        public TResult Result { get; }
    }
}
