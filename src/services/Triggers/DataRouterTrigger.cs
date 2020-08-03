using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace Insights.Data.Triggers
{
    public static class DataRouterTrigger
    {
        [FunctionName("DataRouterTrigger")]
        public static async Task RunAsync([ServiceBusTrigger("partnerfilesreceivedevent", "trigger")]string messageBody, CancellationToken token, ILogger log, ExecutionContext context)
        {
            await new DataBricksJob("datarouter", context, messageBody, token, log).RunPythonJob();
            
            log.LogInformation($"DataRouterTrigger: Message processed\n{messageBody}");
        }
    }
}
