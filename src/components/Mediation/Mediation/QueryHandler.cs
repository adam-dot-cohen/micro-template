using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Laso.Mediation
{
    public abstract class QueryHandler<TResult> : IRequestHandler<IQuery<TResult>, Response<TResult>>
    {
        public abstract Task<Response<TResult>> Handle(IQuery<TResult> request, CancellationToken cancellationToken);

        protected static Response<TResult> Succeeded(TResult result) { return new Response<TResult> { IsValid = true, Result = result }; }
        protected static Response<TResult> Failed(string message) { return new Response<TResult> { IsValid = false, ValidationMessages = new List<ValidationMessage> { new ValidationMessage(string.Empty, message) } }; }
        protected static Response<TResult> Failed(string key, string message) { return new Response<TResult> { IsValid = false, ValidationMessages = new List<ValidationMessage> { new ValidationMessage(key, message) } }; }
        protected static Response<TResult> Failed(params ValidationMessage[] messages) { return new Response<TResult> { IsValid = false, ValidationMessages = messages }; }
        protected static Response<TResult> Failed(Exception exception) { return new Response<TResult> { IsValid = false, Exception = exception }; }
        protected static Response<TResult> Failed(Response response) { return new Response<TResult> { IsValid = false, Exception = response.Exception, ValidationMessages = response.ValidationMessages }; }
    }
}