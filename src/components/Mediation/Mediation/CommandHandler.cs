using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Laso.Mediation
{
    public interface ICommandHandler<in TCommand, TResult> : IRequestHandler<TCommand, CommandResponse<TResult>>
        where TCommand : ICommand<TResult>
    {
    }

    public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, CommandResponse>
        where TCommand : ICommand
    {
    }

    public abstract class CommandHandler<TCommand, TResult> : ICommandHandler<TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        public abstract Task<CommandResponse<TResult>> Handle(TCommand request, CancellationToken cancellationToken);

        protected static CommandResponse<TResult> Succeeded(TResult result) => CommandResponse.Succeeded(result);
        protected static CommandResponse<TResult> Failed(string message) => CommandResponse.Failed<TResult>(message);
        protected static CommandResponse<TResult> Failed(string key, string message) => CommandResponse.Failed<TResult>(key, message);
        protected static CommandResponse<TResult> Failed(ValidationMessage message) => CommandResponse.Failed<TResult>(message);
        protected static CommandResponse<TResult> Failed(IEnumerable<ValidationMessage> messages) => CommandResponse.Failed<TResult>(messages);
        protected static CommandResponse<TResult> Failed(Exception exception) => CommandResponse.Failed<TResult>(exception);
        protected static CommandResponse<TResult> Failed(Response response) => CommandResponse.Failed<TResult>(response);
    }

    public abstract class CommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public abstract Task<CommandResponse> Handle(TCommand request, CancellationToken cancellationToken);

        protected static CommandResponse Succeeded() => CommandResponse.Succeeded();

        protected static CommandResponse Failed(string message) => CommandResponse.Failed<Unit>(message);
        protected static CommandResponse Failed(string key, string message) => CommandResponse.Failed<Unit>(key, message);
        protected static CommandResponse Failed(ValidationMessage message) => CommandResponse.Failed<Unit>(message);
        protected static CommandResponse Failed(IEnumerable<ValidationMessage> messages) => CommandResponse.Failed<Unit>(messages);
        protected static CommandResponse Failed(Exception exception) => CommandResponse.Failed<Unit>(exception);
        protected static CommandResponse Failed(Response response) => CommandResponse.Failed<Unit>(response);
    }
}