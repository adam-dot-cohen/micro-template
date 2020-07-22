using System;
using System.Collections.Generic;

namespace Laso.Mediation
{
    public abstract class QueryResponse : Response
    {
        public static QueryResponse<TResult> Succeeded<TResult>(TResult result) => new QueryResponse<TResult> { Result = result };

        public static QueryResponse<TResult> Failed<TResult>(string message) => new QueryResponse<TResult> { ValidationMessages = new List<ValidationMessage> { new ValidationMessage(string.Empty, message) } };
        public static QueryResponse<TResult> Failed<TResult>(string key, string message) => new QueryResponse<TResult> { ValidationMessages = new List<ValidationMessage> { new ValidationMessage(key, message) } };
        public static QueryResponse<TResult> Failed<TResult>(params ValidationMessage[] messages) => new QueryResponse<TResult> { ValidationMessages = messages };
        public static QueryResponse<TResult> Failed<TResult>(Exception exception) => new QueryResponse<TResult> { Exception = exception };
        public static QueryResponse<TResult> Failed<TResult>(Response response)
        {
            if (response.Success())
            {
                throw new Exception($"Expected failure response of type: {response.GetType().Name}");
            }

            return new QueryResponse<TResult> {Exception = response.Exception, ValidationMessages = response.ValidationMessages};
        }
    }

    public class QueryResponse<TResult> : QueryResponse
    {
        public TResult Result { get; set; }
    }
}
