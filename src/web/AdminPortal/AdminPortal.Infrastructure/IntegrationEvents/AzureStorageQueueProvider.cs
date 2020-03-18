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
        private const int MaxQueueNameLength = 63;

        private readonly AzureStorageQueueOptions _options;

        public AzureStorageQueueProvider(AzureStorageQueueOptions options)
        {
            _options = options;
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
            var name = _options.QueueNameFormat
                .Replace("{MachineName}", Environment.MachineName)
                .Replace("{EventName}", queueName);

            name = new string(name.ToLower()
                .Where(x => char.IsLetterOrDigit(x) || x == '-')
                .SkipWhile(char.IsPunctuation)
                .ToArray())
                .Truncate(MaxQueueNameLength);

            return name;
        }

        protected virtual async Task<QueueClient> GetQueue(string queueName, CancellationToken cancellationToken)
        {
            QueueClient client;

            if (string.IsNullOrWhiteSpace(_options.EndpointUrl))
            {
                client = new QueueClient(_options.ConnectionString, queueName);
            }
            else
            {
                var queueUri = new Uri(_options.EndpointUrl.Trim().If(x => !x.EndsWith("/"), x => x + "/") + queueName);

                client = new QueueClient(queueUri, new DefaultAzureCredential());
            }

            try
            {
                await client.CreateAsync(cancellationToken: cancellationToken);
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == QueueErrorCode.QueueAlreadyExists)
            {

            } //TODO: change to CreateIfNotExistsAsync when available: https://github.com/Azure/azure-sdk-for-net/issues/7879

            return client;
        }
    }

    public class AzureStorageQueueOptions
    {
        public static readonly string Section = "AzureStorageQueue";

        public string ConnectionString { get; set; }
        public string EndpointUrl { get; set; }
        public string QueueNameFormat { get; set; } = "{EventName}";
    }
}