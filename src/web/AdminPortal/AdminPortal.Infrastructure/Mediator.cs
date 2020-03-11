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
            var plan = new MediatorPlan<CommandResponse>(_serviceProvider, typeof(IQueryHandler<,>), command.GetType());
            return plan.InvokeCommand(command, cancellationToken);
        }

        public Task<QueryResponse<TResult>> Query<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
        {
            var plan = new MediatorPlan<TResult>(_serviceProvider, typeof(IQueryHandler<,>), query.GetType());
            return plan.InvokeQuery(query, cancellationToken);
        }

            private class MediatorPlan<TResult>
        {
            private readonly MethodInfo _handleMethod;
            private readonly Func<object> _getHandler;

            public MediatorPlan(IServiceProvider serviceProvider, Type handlerTypeTemplate, Type messageType)
            {
                var genericHandlerType = handlerTypeTemplate.MakeGenericType(messageType, typeof(TResult));
                var handler = serviceProvider.GetService(genericHandlerType);
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

            public Task<QueryResponse<TResult>> InvokeQuery(IQuery<TResult> message, CancellationToken cancellationToken)
            {
                return (Task<QueryResponse<TResult>>)_handleMethod
                    .Invoke(_getHandler(), new object[] { message, cancellationToken });
            }

            public Task<CommandResponse> InvokeCommand(ICommand message, CancellationToken cancellationToken)
            {
                return (Task<CommandResponse>)_handleMethod
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
