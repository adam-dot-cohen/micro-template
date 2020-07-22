using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Laso.Mediation
{
    public abstract class CommandHandler<TCommand, TResult> : IRequestHandler<TCommand, Response<TResult>>
        where TCommand : ICommand<TResult>
    {
        public abstract Task<Response<TResult>> Handle(TCommand request, CancellationToken cancellationToken);

        protected static Response<TResult> Succeeded(TResult result) { return new Response<TResult> { IsValid = true, Result = result }; }
        protected static Response<TResult> Failed(string message) { return new Response<TResult> { IsValid = false, ValidationMessages = new List<ValidationMessage> { new ValidationMessage(string.Empty, message) } }; }
        protected static Response<TResult> Failed(string key, string message) { return new Response<TResult> { IsValid = false, ValidationMessages = new List<ValidationMessage> { new ValidationMessage(key, message) } }; }
        protected static Response<TResult> Failed(params ValidationMessage[] messages) { return new Response<TResult> { IsValid = false, ValidationMessages = messages }; }
        protected static Response<TResult> Failed(Exception exception) { return new Response<TResult> { IsValid = false, Exception = exception }; }
        protected static Response<TResult> Failed(Response response) { return new Response<TResult> { IsValid = false, Exception = response.Exception, ValidationMessages = response.ValidationMessages }; }
    }

    public abstract class CommandHandler<TCommand> : IRequestHandler<TCommand, Response>
        where TCommand : ICommand
    {
        public abstract Task<Response> Handle(TCommand request, CancellationToken cancellationToken);

        protected static Response Succeeded() { return new Response { IsValid = true }; }
        protected static Response Failed(string message) { return new Response { IsValid = false, ValidationMessages = new List<ValidationMessage> { new ValidationMessage(string.Empty, message) } }; }
        protected static Response Failed(string key, string message) { return new Response { IsValid = false, ValidationMessages = new List<ValidationMessage> { new ValidationMessage(key, message) } }; }
        protected static Response Failed(params ValidationMessage[] messages) { return new Response { IsValid = false, ValidationMessages = messages }; }
        protected static Response Failed(Exception exception) { return new Response { IsValid = false, Exception = exception }; }
        protected static Response Failed(Response response) { return new Response { IsValid = false, Exception = response.Exception, ValidationMessages = response.ValidationMessages }; }
    }
}