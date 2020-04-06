using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Mediator;
using Microsoft.Extensions.Logging;

namespace Laso.AdminPortal.Infrastructure
{
    public class Mediator : IMediator
    {
        private const string HandleMethod = "Handle";

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Mediator> _logger;

        public Mediator(IServiceProvider serviceProvider, ILogger<Mediator> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<CommandResponse> Command(ICommand command, CancellationToken cancellationToken)
        {
            var messageType = command.GetType();
            var plan = new MediatorPlan(_serviceProvider, typeof(ICommandHandler<>), messageType);
            var responseTask = (Task<CommandResponse>)plan.Invoke(command, cancellationToken);
            var response = await responseTask;
            LogIfError(messageType, response);

            return response;
        }

        public async Task<CommandResponse<TResult>> Command<TResult>(ICommand<TResult> command, CancellationToken cancellationToken)
        {
            var messageType = command.GetType();
            var plan = new MediatorPlan(_serviceProvider, typeof(ICommandHandler<,>), messageType, typeof(TResult));
            var responseTask = (Task<CommandResponse<TResult>>)plan.Invoke(command, cancellationToken);
            var response = await responseTask;
            LogIfError(messageType, response);

            return response;
        }

        public async Task<QueryResponse<TResult>> Query<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
        {
            var messageType = query.GetType();
            var plan = new MediatorPlan(_serviceProvider, typeof(IQueryHandler<,>), messageType, typeof(TResult));
            var responseTask = (Task<QueryResponse<TResult>>)plan.Invoke(query, cancellationToken);
            var response = await responseTask;
            LogIfError(messageType, response);

            return response;
        }

        private class MediatorPlan
        {
            private readonly MethodInfo _handleMethod;
            private readonly Func<object> _getHandler;

            public MediatorPlan(IServiceProvider serviceProvider, Type handlerTypeTemplate, Type messageType, Type resultType = null)
            {
                var genericHandlerType = (resultType == null) 
                    ? handlerTypeTemplate.MakeGenericType(messageType)
                    : handlerTypeTemplate.MakeGenericType(messageType, resultType);

                var handler = serviceProvider.GetService(genericHandlerType);

                if (handler == null)
                    throw new InvalidOperationException($"Handler not found for {messageType.Name}.");

                var needsNewHandler = false;

                _handleMethod = GetHandlerMethod(genericHandlerType, HandleMethod, messageType);
                _getHandler = () =>
                {
                    if (needsNewHandler)
                        handler = serviceProvider.GetService(genericHandlerType);

                    needsNewHandler = true;

                    return handler;
                };
            }

            public object Invoke(IMessage message, CancellationToken cancellationToken)
            {
                return _handleMethod
                    .Invoke(_getHandler(), new object[] { message, cancellationToken });
            }

            private static MethodInfo GetHandlerMethod(Type handlerType, string handlerMethodName, Type messageType)
            {
                return handlerType.GetMethod(
                    handlerMethodName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod,
                    null, 
                    CallingConventions.HasThis,
                    new[] { messageType, typeof(CancellationToken) },
                    null);
            }
        }

        private void LogIfError(Type messageType, Response response)
        {
            if (response.Success)
            {
                return;
            }

            var errorContext = new
            {
                MessageType = messageType.Name,
                ValidationMessages = response.ValidationMessages ?? new List<ValidationMessage>()
            };
            if (response.Exception == null)
            {
                _logger.LogWarning("Handler Error: {@HandlerResultDetails}", errorContext);
            }
            else
            {
                _logger.LogError(response.Exception, "Handler Error: {@HandlerResultDetails}", errorContext);
            }
        }
    }
}
