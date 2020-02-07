using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using DataImport.Subscriptions.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DataImport.Subscriptions.Api
{
    public static class ImportSubscriptionHttpTrigger
    {
        [FunctionName(nameof(GetImportSubscriptionById))]
        public static async Task<IActionResult> GetImportSubscriptionById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "ImportSubscriptions/{id}")] HttpRequest req,
            ILogger log,
            string id)
        {
            // awaits here and below just to silence warnings
            // as we'll eventually be getting this stuff async
            var subscription = await Task.Run(() => new ImportSubscription
            {
                Id = id,
                ExportFrom = new Partner
                {
                    Id = "1",
                    Name = "Quarterspot"
                },
                Frequency = ImportFrequency.Weekly.ToString(),
                Imports = (ImportType[])Enum.GetValues(typeof(ImportType)),
                LastSuccessfulImport = null,
                NextScheduledImport = DateTime.Now.AddDays(-1)
            });

            return new OkObjectResult(subscription);
        }

        [FunctionName(nameof(GetImportSubscriptionsByPartnerId))]
        public static async Task<IActionResult> GetImportSubscriptionsByPartnerId(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Partners/{partnerId}/ImportSubscriptions")] HttpRequest req,
            ILogger log,
            string partnerId)
        {
            var subscriptions = await Task.Run(() => new []
                {
                    new ImportSubscription
                    {
                        Id = "1",
                        ExportFrom = new Partner
                        {
                            Id = partnerId,
                            Name = "Quarterspot"
                        },
                        Frequency = ImportFrequency.Daily.ToString(),
                        Imports = new []
                        {
                            ImportType.Demographic
                        },
                        LastSuccessfulImport = null,
                        NextScheduledImport = DateTime.Now.AddDays(-1)
                    },
                    new ImportSubscription
                    {
                        Id = "2",
                        ExportFrom = new Partner
                        {
                            Id = partnerId,
                            Name = "Quarterspot"
                        },
                        Frequency = ImportFrequency.Weekly.ToString(),
                        Imports = new []
                        {
                            ImportType.Firmographic
                        },
                        LastSuccessfulImport = null,
                        NextScheduledImport = DateTime.Now.AddDays(-1)
                    }
            });

            return new OkObjectResult(subscriptions);
        }
    }
}
