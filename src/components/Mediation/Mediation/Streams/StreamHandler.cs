using System;
using System.Collections.Generic;
using System.Threading;
using Infrastructure.Mediation.Validation;
using MediatR;

namespace Infrastructure.Mediation.Stream
{
    public interface IStreamHandler<in TRequest, TResult> : IStreamRequestHandler<TRequest, StreamResponse<TResult>>
        where TRequest : IStreamRequest<StreamResponse<TResult>> { }

    public abstract class StreamHandler<TRequest, TResult> : IStreamHandler<TRequest, TResult>
        where TRequest : IStreamRequest<StreamResponse<TResult>>
    {
        public abstract IAsyncEnumerable<StreamResponse<TResult>> Handle(TRequest request, CancellationToken cancellationToken);
        protected static StreamResponse<TResult> Succeeded(TResult result) => StreamResponse.Succeeded(result);
        protected static StreamResponse<TResult> Failed(string message) => StreamResponse.Failed<TResult>(message);
        protected static StreamResponse<TResult> Failed(string key, string message) => StreamResponse.Failed<TResult>(key, message);
        protected static StreamResponse<TResult> Failed(ValidationMessage message) => StreamResponse.Failed<TResult>(message);
        protected static StreamResponse<TResult> Failed(IEnumerable<ValidationMessage> messages) => StreamResponse.Failed<TResult>(messages);
        protected static StreamResponse<TResult> Failed(Exception exception) => StreamResponse.Failed<TResult>(exception);
        protected static StreamResponse<TResult> Failed(Response response) => StreamResponse.Failed<TResult>(response);
    }
}