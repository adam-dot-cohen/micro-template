using System;
using System.Collections.Generic;
using Infrastructure.Mediation.Validation;

namespace Infrastructure.Mediation.Stream
{
    public abstract class StreamResponse : Response
    {
        protected StreamResponse(IEnumerable<ValidationMessage> failures = null, Exception exception = null) : base(failures, exception) { }
        public static StreamResponse<TResult> Succeeded<TResult>(TResult result) => new(result: result);
        public static StreamResponse<TResult> Failed<TResult>(string message) => new(new[] {new ValidationMessage(string.Empty, message)});
        public static StreamResponse<TResult> Failed<TResult>(string key, string message) => new(new[] {new ValidationMessage(key, message)});
        public static StreamResponse<TResult> Failed<TResult>(ValidationMessage message) => new(new [] {message});
        public static StreamResponse<TResult> Failed<TResult>(IEnumerable<ValidationMessage> messages) => new(messages);
        public static StreamResponse<TResult> Failed<TResult>(Exception exception) => new(exception: exception);
        public static StreamResponse<TResult> Failed<TResult>(Response response)
        {
            if (response.Success)
            {
                throw new Exception($"Expected failure response of type: {response.GetType().Name}");
            }

            return response.ToResponse<StreamResponse<TResult>>();
        }

        public static StreamResponse<TResult> From<TResult>(params Response[] responses)
           => Response.From<StreamResponse<TResult>>(responses);
    }

    public class StreamResponse<TResult> : StreamResponse
    {
        public StreamResponse() { }
        public StreamResponse(IEnumerable<ValidationMessage> failures = null, Exception exception = null, TResult result = default) : base(failures, exception)
            => Result = result;
        public TResult Result { get; }
    }
}
