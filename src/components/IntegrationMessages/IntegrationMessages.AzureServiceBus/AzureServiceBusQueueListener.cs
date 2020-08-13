using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Laso.IntegrationMessages;
using Laso.IO.Serialization;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IntegrationMessages.AzureServiceBus
{
    public class AzureServiceBusQueueListener<T> : IHostedService, IDisposable where T : IIntegrationMessage
    {
        private readonly ICommandHandler<T> _handler;
        private readonly AzureServiceBusQueueProvider _queueProvider;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogger<AzureServiceBusQueueListener<T>> _logger;

        private readonly CancellationTokenSource _stopSource = new CancellationTokenSource();
        private readonly string _queueName;

        private Task _listener;
        private QueueClient _queueClient;

        public AzureServiceBusQueueListener(ILogger<AzureServiceBusQueueListener<T>> logger, ICommandHandler<T> handler, AzureServiceBusQueueProvider queueProvider, IJsonSerializer jsonSerializer)
        {
            _logger = logger;
            _handler = handler;
            _queueProvider = queueProvider;
            _jsonSerializer = jsonSerializer;
            _queueName = _queueProvider.GetQueueName(typeof(T));
        }

        public async Task ListenForMessages(CancellationToken stopToken)
        {
            stopToken.Register(() => Task.WaitAll(Close()));

            while (!stopToken.IsCancellationRequested && _queueClient == null)
            {
                try
                {
                    var client = await _queueProvider.GetQueueClient(typeof(T), stopToken);
                    RegisterMessageHandler(client, stopToken);
                    _queueClient = client;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"{_queueName} handler thew an exception");
                    
                    await Task.Delay(TimeSpan.FromSeconds(10), stopToken);
                }
            }
        }

        private void RegisterMessageHandler(IReceiverClient client, CancellationToken stopToken)
        {
            var msgOptions = new MessageHandlerOptions(async x => 
            { 
                _logger.LogCritical(x.Exception, $"Error attempting to handle {_queueName} message.  Restarting Listener ...");
                if(_queueClient != null)
                {
                    await Close();
                    await ListenForMessages(stopToken);
                }
            }) { AutoComplete = false, MaxConcurrentCalls = 1};

            client.RegisterMessageHandler(async (x,y) => 
            {
                if(client.IsClosedOrClosing)
                    return;
                await HandleMessageAsync(client, x, stopToken);
            }, msgOptions);
        }

        private async Task HandleMessageAsync(IReceiverClient client, Message message, CancellationToken stopToken)
        {
            try
            {
                var queueMsg = _jsonSerializer.DeserializeFromUtf8Bytes<T>(message.Body);
                await _handler.Handle(queueMsg, stopToken);
                await client.CompleteAsync(message.SystemProperties.LockToken);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, $"Exception handling {_queueName} message");
                try
                {
                    //Let the queue's retry policy decide if this should be retried or dead lettered
                    await client.AbandonAsync(message.SystemProperties.LockToken, new Dictionary<string, object>{{ "Reason", e.Message } });
                }
                catch (Exception abandonException)
                {
                    _logger.LogCritical(abandonException, $"Exception while attempting to abandon {_queueName} message");
                }
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _listener = ListenForMessages(_stopSource.Token);

            if (_listener.IsCompleted)
            {
                _logger.LogWarning($"{_queueName} exited prematurely.");
            }

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_listener == null)
            {
                return;
            }

            try
            {
                _stopSource.Cancel();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An exception occurred when attempting to gracefully exit {_queueName} listener.");
            }
            finally
            {
                await Task.WhenAny(_listener, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        public void Dispose()
        {
            _stopSource.Cancel();
        }

        private async Task Close()
        {
            if (_queueClient == null || _queueClient.IsClosedOrClosing)
            {
                _queueClient = null;
                return;
            }

            try
            {
                await _queueClient.CloseAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while attempting to close {_queueName} queue listener");
            }
            _queueClient = null;
        }
    }
}
