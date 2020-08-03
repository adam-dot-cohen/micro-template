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
            await new DataBricksJob("dataquality", context, messageBody, token, log).RunPythonJob();

            log.LogInformation($"DataQualityTrigger: Message processed\n{messageBody}");

        }
    }
}
