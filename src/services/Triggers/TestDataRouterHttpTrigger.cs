using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Insights.Data.Triggers.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Insights.Data.Triggers
{
    public class TestDataRouterHttpTrigger
    {
        private readonly IDataBricksJobService _dataBricksJobService;

        public TestDataRouterHttpTrigger(IDataBricksJobService dataBricksJobService)
        {
            _dataBricksJobService = dataBricksJobService;
        }

        // Used for local testing
        [FunctionName(nameof(TestDataRouterHttpTrigger))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] string messageBody,
            [Blob("%jobstateblobpath_datarouter%", FileAccess.Read)] Stream jobStateStream,
            ILogger log,
            CancellationToken cancellationToken)
        {
            messageBody = string.IsNullOrEmpty(messageBody) ? @"
{
  ""CorrelationId"": ""2fc71727-8f6d-4fef-9212-8c4f6c00eb83"",
  ""Files"": [
    {
      ""DataCategory"": ""AccountTransaction"",
      ""ETag"": ""0x8D7D4EE81DE99E8"",
      ""Id"": ""c7f47877-1382-42a7-8398-a0a3a43473ee"",
      ""Policy"": """",
      ""Uri"": ""abfss://raw@lasodevinsights.dfs.core.windows.net/93383d2d-07fd-488f-938b-f9ce1960fee3/2020/202003/20200330/a22cdb4a-fda5-4296-aa6b-72e695aac0da_AccountTransaction.csv"",
      ""fromDict"": {}
    }
  ],
  ""OrchestrationId"": ""a22cdb4a-fda5-4296-aa6b-72e695aac0da"",
  ""PartnerId"": ""93383d2d-07fd-488f-938b-f9ce1960fee3"",
  ""PartnerName"": ""Demo Partner"",
  ""Timestamp"": ""2020-03-30T21:08:35.107901+00:00""
}
" : messageBody;

            var jobState = await JsonSerializer.DeserializeAsync<JobState>(jobStateStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken);
            var result = await _dataBricksJobService.RunPythonJob(messageBody, jobState, cancellationToken);

            log.LogInformation($"DataRouterTrigger: Message processed\n{messageBody}");

            return new OkObjectResult(result);
        }
    }
}
