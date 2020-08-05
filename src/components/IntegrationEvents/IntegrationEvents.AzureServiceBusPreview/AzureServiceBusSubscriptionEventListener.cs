using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Laso.IntegrationEvents.AzureServiceBus.Preview.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Laso.IntegrationEvents.AzureServiceBus.Preview
{
    public class AzureServiceBusSubscriptionEventListener<T>
    {
        private readonly AzureServiceBusTopicProvider _topicProvider;
        private readonly string _subscriptionName;
        private readonly IListenerMessageHandler<T> _listenerMessageHandler;
        private readonly string _topicName;
        private readonly string _sqlFilter;
        private readonly ILogger<AzureServiceBusSubscriptionEventListener<T>> _logger;

        private Func<ServiceBusProcessor> _getProcessor;
        private ServiceBusProcessor _processor;

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
            await Initialize(stoppingToken);

            while (!stoppingToken.IsCancellationRequested && _processor == null)
            {
                try
                {
                    var processor = _getProcessor();

                    RegisterMessageHandler(processor, stoppingToken);

                    await processor.StartProcessingAsync(stoppingToken);

                    _processor = processor;
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "Exception attempting to configure topic subscription.");

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
        }

        private async Task Initialize(CancellationToken stoppingToken)
        {
            if (_getProcessor != null)
                return;

            var options = new ServiceBusProcessorOptions
            {
                AutoComplete = false,
                MaxReceiveWaitTime = TimeSpan.FromSeconds(10)
            };

            _getProcessor = await _topicProvider.GetProcessorFactory(_topicName ?? typeof(T).Name, _subscriptionName, _sqlFilter, options, stoppingToken);

            stoppingToken.Register(() => Task.WaitAll(Close()));
        }

        private void RegisterMessageHandler(ServiceBusProcessor processor, CancellationToken stoppingToken)
        {
            processor.ProcessErrorAsync += async x =>
            {
                _logger.LogCritical(x.Exception, "Exception attempting to handle message. Restarting listener.");

                await Close();

                await Open(stoppingToken);
            };

            processor.ProcessMessageAsync += async x =>
            {
                if (!processor.IsProcessing)
                    return;

                await ProcessEvent(x, stoppingToken);
            };
        }

        protected virtual async Task<EventProcessingResult<T>> ProcessEvent(ProcessMessageEventArgs args, CancellationToken stoppingToken)
        {
            var result = new EventProcessingResult<T> { Message = args.Message };

            try
            {
                await _listenerMessageHandler.Handle(result.Message, result, stoppingToken);

                await args.CompleteMessageAsync(result.Message, stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Exception attempting to handle message.");

                result.Exception = e;

                try
                {
                    if (result.Message.DeliveryCount >= 3)
                    {
                        await args.DeadLetterMessageAsync(result.Message, "Exceeded retries", e.InnermostException().Message, stoppingToken);

                        result.WasDeadLettered = true;
                    }
                    else
                    {
                        await args.AbandonMessageAsync(result.Message, cancellationToken: stoppingToken);

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
            if (_processor == null || !_processor.IsProcessing)
            {
                _processor = null;

                return;
            }

            try
            {
                //Underlying AMQP library does not support cancellation tokens
                //thus this will wait for a timeout of up to ServiceBusProcessorOptions.MaxReceiveWaitTime (default 60 seconds)
                //See https://github.com/Azure/azure-amqp/blob/4dcaf85d6073902c62b6866112447020321095da/src/ReceivingAmqpLink.cs#L140
                await _processor.StopProcessingAsync();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Exception attempting to stop message processing.");
            }

            _processor = null;
        }
    }
}
