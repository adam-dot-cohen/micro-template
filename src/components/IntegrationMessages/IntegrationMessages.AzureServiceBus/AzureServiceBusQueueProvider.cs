using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace IntegrationMessages.AzureServiceBus
{
    public class AzureServiceBusQueueProvider 
    {
        private readonly AzureServiceBusMessageConfiguration _configuration;

        public AzureServiceBusQueueProvider(AzureServiceBusMessageConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetQueueName(Type cmdType)
        {
            var cmdName = cmdType.Name;
            var index = cmdName.IndexOf('`');

            if(index > 0)
                cmdName = cmdName.Substring(0, index);

            var name = _configuration.QueueNameFormat
                .Replace("{MachineName}", Environment.MachineName)
                .Replace("{CommandName}", cmdName);

            name = new string(name.ToLower()
                .Where(x => char.IsLetterOrDigit(x) || x == '-' || x == '.' || x == '_')
                .SkipWhile(char.IsPunctuation)
                .ToArray());

            return name;
        }

        public async Task<QueueClient> GetQueueClient(Type commandType, CancellationToken stopToken)
        {
            var queueName = GetQueueName(commandType);
            var management = new ManagementClient(_configuration.ConnectionString);
            QueueDescription queue;

            if(!await management.QueueExistsAsync(queueName, stopToken))
            {
                var description = new QueueDescription(queueName)
                {
                    MaxDeliveryCount = 3,
                    EnableDeadLetteringOnMessageExpiration = true
                };
                queue = await management.CreateQueueAsync(description, stopToken);
            }
            else
            {
                queue = await management.GetQueueAsync(queueName, stopToken);
            }

            return new QueueClient(_configuration.ConnectionString, queue.Path, ReceiveMode.PeekLock, RetryPolicy.Default);
        }
    }
}
