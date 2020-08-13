using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.IntegrationEvents.AzureServiceBus.Extensions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Laso.IntegrationEvents.AzureServiceBus
{
    public class AzureServiceBusSubscriptionEventListener<T>
    {
        private readonly AzureServiceBusTopicProvider _topicProvider;
        private readonly string _subscriptionName;
        private readonly IListenerMessageHandler<T> _listenerMessageHandler;
        private readonly string _topicName;
        private readonly string _sqlFilter;
        private readonly ILogger<AzureServiceBusSubscriptionEventListener<T>> _logger;

        private SubscriptionClient _client;

        public AzureServiceBusSubscriptionEventListener(
            AzureServiceBusTopicProvider topicProvider,
            string subscriptionName,
            IListenerMessageHandler<T> listenerMessageHandler,
            string topicName = null,
            string sqlFilter = null,
            ILogger<AzureServiceBusSubscriptionEventListener<T>> logger = null)
        {
            _topicProvider = topicProvider;
            _subscriptionName = subscriptionName;
            _listenerMessageHandler = listenerMessageHandler;
            _topicName = topicName;
            _sqlFilter = sqlFilter;
            _logger = logger ?? new NullLogger<AzureServiceBusSubscriptionEventListener<T>>();
        }

        public async Task Open(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => Task.WaitAll(Close()));

            while (!stoppingToken.IsCancellationRequested && _client == null)
            {
                try
                {
                    var client = await _topicProvider.GetSubscriptionClient(_topicName ?? typeof(T).Name, _subscriptionName, _sqlFilter, stoppingToken);

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

        protected virtual async Task<EventProcessingResult<T>> ProcessEvent(IReceiverClient client, Message message, CancellationToken stoppingToken)
        {
            var result = new EventProcessingResult<T> { Message = message };

            try
            {
                await _listenerMessageHandler.Handle(result.Message, result, stoppingToken);

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
                        await client.DeadLetterAsync(result.Message.SystemProperties.LockToken, "Exceeded retries", e.InnermostException().Message);

                        result.WasDeadLettered = true;
                    }
                    else
                    {
                        await client.AbandonAsync(result.Message.SystemProperties.LockToken);

                        result.WasAbandoned = true;
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
