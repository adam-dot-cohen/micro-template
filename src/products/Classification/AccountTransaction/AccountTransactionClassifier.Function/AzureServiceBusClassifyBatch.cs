using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Insights.AccountTransactionClassifier.Function
{
    public class AzureServiceBusClassifyBatch
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureServiceBusClassifyBatch> _logger;

        public AzureServiceBusClassifyBatch(IConfiguration configuration, ILogger<AzureServiceBusClassifyBatch> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [FunctionName("AzureServiceBusClassifyBatch")]
        public Task Run(
            [ServiceBusTrigger("scheduling", "AcctTxnClassifier.Function")] string msg,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation($"AzureServiceBusClassifyBatch topic trigger function processing message: {msg}");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var message = JsonSerializer.Deserialize<ExperimentRunScheduledEvent>(msg, options);
            var evt = message.Data;
            if (evt == null)
                return Task.CompletedTask;

            var now = DateTime.UtcNow;
            var process = new ClassifyBatchProcess();

            return process.Run(
                evt.PartnerId, 
                Path.GetFileName(evt.Files.First().Uri),
                $"Laso_QuarterSpot_R_AccountTransaction_{now:yyyyMMdd}_{now:yyyyMMddHHmmss}.csv",
                _configuration,
                _logger,
                cancellationToken);
        }
    }

    public abstract class CloudEventEnvelope<TData>
        where TData : class
    {
        public string Id { get; set; } = null!;
        public Uri Source { get; set; } = null!;
        public string SpecVersion { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string DataSchema { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public DateTime Time { get; set; }
        public TData Data { get; set; } = null!;
    }

    public class ExperimentRunScheduledEvent : CloudEventEnvelope<ExperimentRunScheduledEventV1>
    {
    }

    public class ExperimentRunScheduledEventV1
    {
        public string PartnerId { get; set; } = null!;
        public string ExperimentScheduleId { get; set; } = null!;
        public string FileBatchId { get;  set; } = null!;

        public IReadOnlyCollection<ExperimentFile> Files { get; set; } = null!;
    }

    public class ExperimentFile
    {
        public string Uri { get; set; } = null!;

        public string? DataCategory { get; set; }
    }
}
