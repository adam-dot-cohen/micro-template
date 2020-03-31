using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace Insights.Data.Triggers
{
    public static class DataQualityTrigger
    {
        [FunctionName("DataQualityTrigger")]
        public static async Task RunAsync([ServiceBusTrigger("dataqualitycommand", "trigger")]string messageBody, CancellationToken token, ILogger log, ExecutionContext context)
        {
            var config = new ConfigHelper(context).Config;
            var jobId = config.GetValue<Int64>("jobId_dataquality");
            var uriRoot = config.GetValue<string>("uriRoot");
            var bearerToken = config.GetValue<string>("bearerToken");

            var result = await DataBricksHelper.RunPythonJob(uriRoot, bearerToken, messageBody, jobId);
            if (result.Success == false)
            {
                log.LogError($"DataQualityTrigger: Failed to submit job: {result.Message}");
            }
            else
            {
                log.LogInformation($"DataQualityTrigger: Job successfully submitted", result.Response);
            }
            
            log.LogInformation($"DataQualityTrigger: Message processed {messageBody}");
        }
    }
}
