using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Insights.Data.Triggers
{
    public class DataQualityTrigger
    {
        private readonly IDataBricksJobService _dataBricksJobService;

        public DataQualityTrigger(IDataBricksJobService dataBricksJobService)
        {
            _dataBricksJobService = dataBricksJobService;
        }

        [FunctionName(nameof(DataQualityTrigger))]
        public async Task RunAsync(
            [ServiceBusTrigger("dataqualitycommand", "trigger")] string messageBody,
            [Blob("%jobstateblobpath_dataquality%", FileAccess.Read)] Stream jobStateStream,
            ILogger log,
            CancellationToken cancellationToken)
        {
            var jobState = await JsonSerializer.DeserializeAsync<JobState>(jobStateStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken);
            await _dataBricksJobService.RunPythonJob(messageBody, jobState, cancellationToken);

            log.LogInformation($"DataQualityTrigger: Message processed\n{messageBody}");

        }
    }
}