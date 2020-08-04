using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.IntegrationMessages;
using Laso.IO.Serialization;
using Microsoft.Azure.ServiceBus;

namespace IntegrationMessages.AzureServiceBus
{
    public class AzureServiceBusMessageSender : IMessageSender
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly AzureServiceBusQueueProvider _queueProvider;

        public AzureServiceBusMessageSender(AzureServiceBusQueueProvider queueProvider, IJsonSerializer jsonSerializer)
        {
            _queueProvider = queueProvider;
            _jsonSerializer = jsonSerializer;
        }

        public async Task Send<T>(T command) where T : IIntegrationMessage
        {
            var fiveSecondsIsLongEnough = new CancellationTokenSource();
            fiveSecondsIsLongEnough.CancelAfter(TimeSpan.FromSeconds(5));
            var client = await _queueProvider.GetQueueClient(command.GetType(), fiveSecondsIsLongEnough.Token);

            var bytes = _jsonSerializer.SerializeToUtf8Bytes(command);

            await client.SendAsync(new Message(bytes));
        }
    }
}
