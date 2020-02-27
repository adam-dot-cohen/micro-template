using System.Text.Json;
using System.Threading.Tasks;
using Laso.Identity.Core.Messaging;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace Laso.Identity.Infrastructure.Eventing
{
    public class AzureServiceBusEventPublisher : IEventPublisher
    {
        private readonly string _connectionString;

        public AzureServiceBusEventPublisher(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task Publish(object @event)
        {
            var managementClient = new ManagementClient(_connectionString);

            var topicName = @event.GetType().Name.ToLower();

            var topic = await managementClient.TopicExistsAsync(topicName)
                ? await managementClient.GetTopicAsync(topicName)
                : await managementClient.CreateTopicAsync(topicName);

            var client = new TopicClient(_connectionString, topic.Path);

            var bytes = JsonSerializer.SerializeToUtf8Bytes(@event);

            await client.SendAsync(new Message(bytes));
        }
    }
}
