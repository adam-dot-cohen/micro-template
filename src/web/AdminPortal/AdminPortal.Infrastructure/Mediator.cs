using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Mediator;

namespace Laso.AdminPortal.Infrastructure
{
    public class Mediator : IMediator
    {
        private const string HandleMethod = "Handle";

        private readonly IServiceProvider _serviceProvider;

        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<CommandResponse> Command(ICommand command, CancellationToken cancellationToken)
        {
            var plan = new MediatorPlan(_serviceProvider, typeof(ICommandHandler<>), command.GetType());
            return (Task<CommandResponse>)plan.Invoke(command, cancellationToken);
        }

        public Task<CommandResponse<TResult>> Command<TResult>(ICommand<TResult> command, CancellationToken cancellationToken)
        {
            var plan = new MediatorPlan(_serviceProvider, typeof(ICommandHandler<,>), command.GetType(), typeof(TResult));
            return (Task<CommandResponse<TResult>>)plan.Invoke(command, cancellationToken);
        }

        public Task<QueryResponse<TResult>> Query<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
        {
            var plan = new MediatorPlan(_serviceProvider, typeof(IQueryHandler<,>), query.GetType(), typeof(TResult));
            return (Task<QueryResponse<TResult>>)plan.Invoke(query, cancellationToken);
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
    }
}
