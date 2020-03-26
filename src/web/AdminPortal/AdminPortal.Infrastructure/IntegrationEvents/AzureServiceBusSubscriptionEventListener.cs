using System;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Extensions;
using Laso.AdminPortal.Infrastructure.Filters;
using Laso.AdminPortal.Infrastructure.Filters.FilterPropertyMappers;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Laso.AdminPortal.Infrastructure.IntegrationEvents
{
    public class AzureServiceBusSubscriptionEventListener<T> : IEventListener
    {
        private readonly AzureServiceBusTopicProvider _topicProvider;
        private readonly string _subscriptionName;
        private readonly Func<T, CancellationToken, Task> _eventHandler;
        private readonly string _sqlFilter;
        private readonly ILogger<AzureServiceBusSubscriptionEventListener<T>> _logger;

        private SubscriptionClient _client;

        public AzureServiceBusSubscriptionEventListener(AzureServiceBusTopicProvider topicProvider, string subscriptionName, Func<T, CancellationToken, Task> eventHandler, Expression<Func<T, bool>> filter, ILogger<AzureServiceBusSubscriptionEventListener<T>> logger = null) : this(topicProvider, subscriptionName, eventHandler, GetSqlFilter(filter), logger) { }
        public AzureServiceBusSubscriptionEventListener(AzureServiceBusTopicProvider topicProvider, string subscriptionName, Func<T, CancellationToken, Task> eventHandler, string sqlFilter = null, ILogger<AzureServiceBusSubscriptionEventListener<T>> logger = null)
        {
            _topicProvider = topicProvider;
            _subscriptionName = subscriptionName;
            _eventHandler = eventHandler;
            _sqlFilter = sqlFilter;
            _logger = logger ?? new NullLogger<AzureServiceBusSubscriptionEventListener<T>>();
        }

        private static string GetSqlFilter(LambdaExpression filterExpression)
        {
            if (filterExpression == null)
                return null;

            //TODO: move mapper construction
            var filterExpressionHelper = new FilterExpressionHelper(
                new IFilterPropertyMapper[] {new EnumFilterPropertyMapper(), new DefaultFilterPropertyMapper() },
                new AzureServiceBusSqlFilterDialect());

            return filterExpressionHelper.GetFilter(filterExpression);
        }

        public async Task Open(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => Task.WaitAll(Close()));

            while (!stoppingToken.IsCancellationRequested && _client == null)
            {
                try
                {
                    var client = await _topicProvider.GetSubscriptionClient(typeof(T), _subscriptionName, _sqlFilter, stoppingToken);

                    RegisterMessageHandler(client, stoppingToken);

                    _client = client;
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "Exception attempting to configure topic subscription.");

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
        }

        private void RegisterMessageHandler(IReceiverClient client, CancellationToken stoppingToken)
        {
            var options = new MessageHandlerOptions(async x =>
            {
                _logger.LogCritical(x.Exception, "Exception attempting to handle message. Restarting listener.");

                if (_client != null)
                {
                    await Close();

                    await Open(stoppingToken);
                }
            }) { AutoComplete = false };

            client.RegisterMessageHandler(async (x, y) =>
            {
                if (client.IsClosedOrClosing)
                    return;

                await ProcessEvent(client, x, stoppingToken);
            }, options);
        }

        protected virtual async Task<EventProcessingResult<Message, T>> ProcessEvent(IReceiverClient client, Message message, CancellationToken stoppingToken)
        {
            var result = new EventProcessingResult<Message, T> { Message = message };

            try
            {
                result.DeserializedMessage = JsonSerializer.Deserialize<T>(new ReadOnlySpan<byte>(result.Message.Body));

                await _eventHandler(result.DeserializedMessage, stoppingToken);

                await client.CompleteAsync(result.Message.SystemProperties.LockToken);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Exception attempting to handle message.");

                result.Exception = e;

                try
                {
                    if (result.Message.SystemProperties.DeliveryCount >= 3)
                    {
                        result.WasDeadLettered = true;

                        await client.DeadLetterAsync(result.Message.SystemProperties.LockToken, "Exceeded retries", e.InnermostException().Message);
                    }
                    else
                    {
                        result.WasAbandoned = true;

                        await client.AbandonAsync(result.Message.SystemProperties.LockToken);
                    }
                }
                catch (Exception secondaryException)
                {
                    result.SecondaryException = secondaryException;

                    _logger.LogCritical(secondaryException, "Exception attempting to abandon/dead letter message.");
                }
            }

            return result;
        }

        private async Task Close()
        {
            if (_client == null || _client.IsClosedOrClosing)
            {
                _client = null;

                return;
            }

            try
            {
                await _client.CloseAsync();
            }
            catch (Exception)
            {
                //TODO: logging
            }

            _client = null;
        }
    }
}
