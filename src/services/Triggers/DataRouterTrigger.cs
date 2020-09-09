using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Insights.Data.Triggers.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Insights.Data.Triggers
{
    public class DataRouterTrigger
    {
        private readonly IDataBricksJobService _dataBricksJobService;

        public DataRouterTrigger(IDataBricksJobService dataBricksJobService)
        {
            _dataBricksJobService = dataBricksJobService;
        }

        [FunctionName(nameof(DataRouterTrigger))]
        public async Task RunAsync(
            [ServiceBusTrigger("partnerfilesreceivedevent", "trigger")] string messageBody,
            [Blob("%jobstateblobpath_datarouter%", FileAccess.Read)] Stream jobStateStream,
            ILogger log,
            CancellationToken cancellationToken)
        {
            var jobState = await JsonSerializer.DeserializeAsync<JobState>(jobStateStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken);
            await _dataBricksJobService.RunPythonJob(messageBody, jobState, cancellationToken);

            log.LogInformation($"DataRouterTrigger: Message processed\n{messageBody}");
        }
    }
}