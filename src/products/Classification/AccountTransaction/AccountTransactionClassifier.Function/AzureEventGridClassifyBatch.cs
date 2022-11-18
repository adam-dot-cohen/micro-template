// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using Azure.Messaging.EventGrid;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;

namespace Insights.AccountTransactionClassifier.Function
{
    public class AzureEventGridClassifyBatch
    {
        private readonly ILogger<AzureEventGridClassifyBatch> _logger;

        public AzureEventGridClassifyBatch(ILogger<AzureEventGridClassifyBatch> logger)
        {
            _logger = logger;
        }

        [FunctionName("AzureEventGridClassifyBatch")]
        public void Run([EventGridTrigger]EventGridEvent eventGridEvent)
        {
            _logger.LogInformation(eventGridEvent.Data.ToString());
        }
    }
}
