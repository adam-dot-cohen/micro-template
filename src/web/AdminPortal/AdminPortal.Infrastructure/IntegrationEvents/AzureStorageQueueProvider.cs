using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Laso.AdminPortal.Core.Extensions;
using Laso.AdminPortal.Infrastructure.Extensions;

namespace Laso.AdminPortal.Infrastructure.IntegrationEvents
{
    public class AzureStorageQueueProvider
    {
        private readonly AzureStorageQueueConfiguration _configuration;

        private readonly ICollection<QueueClient> _createdQueues = new List<QueueClient>();

        public AzureStorageQueueProvider(AzureStorageQueueConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<QueueClient> GetQueue(Type eventType, CancellationToken cancellationToken = default)
        {
            if (eventType.Closes(typeof(IEnumerable<>), out var args))
                eventType = args[0];

            return await GetQueue(GetQueueName(eventType.Name), cancellationToken);
        }

        public async Task<QueueClient> GetDeadLetterQueue(CancellationToken cancellationToken = default)
        {
            return await GetQueue(GetQueueName("DeadLetter"), cancellationToken);
        }

        private string GetQueueName(string queueName)
        {
            var name = _configuration.QueueNameFormat
                .Replace("{MachineName}", Environment.MachineName)
                .Replace("{EventName}", queueName);

            name = new string(name.ToLower()
                .Where(x => char.IsLetterOrDigit(x) || x == '-' || x == '.' || x == '_')
                .SkipWhile(char.IsPunctuation)
                .ToArray());

            return name;
        }

        private async Task<QueueClient> GetQueue(string queueName, CancellationToken cancellationToken)
        {
            QueueClient client;

            if (_configuration.EndpointUrl == "UseDevelopmentStorage=true")
            {
                client = new QueueClient(_configuration.EndpointUrl, queueName);
            }
            else
            {
                var queueUri = new Uri(_configuration.EndpointUrl.Trim().If(x => !x.EndsWith("/"), x => x + "/") + queueName);

                client = new QueueClient(queueUri, new DefaultAzureCredential());
            }

            try
            {
                await client.CreateAsync(cancellationToken: cancellationToken);

                lock (_createdQueues)
                {
                    _createdQueues.Add(client);
                }
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == QueueErrorCode.QueueAlreadyExists) { } //TODO: change to CreateIfNotExistsAsync when available: https://github.com/Azure/azure-sdk-for-net/issues/7879

            return client;
        }

        public async Task DeleteCreatedQueues()
        {
            var tasks = new List<Task>();

            lock (_createdQueues)
            {
                foreach (var queue in _createdQueues)
                    tasks.Add(queue.DeleteAsync());

                _createdQueues.Clear();
            }

            await Task.WhenAll(tasks);
        }
    }

    public class AzureStorageQueueConfiguration
    {
        public string EndpointUrl { get; set; }
        public string QueueNameFormat { get; set; } = "{EventName}";
    }
}