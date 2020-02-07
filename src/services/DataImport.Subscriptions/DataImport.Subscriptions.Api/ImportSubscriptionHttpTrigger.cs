using System;
using System.Threading.Tasks;
using DataImport.Subscriptions.Api.Extensions;
using DataImport.Subscriptions.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DataImport.Subscriptions.Api
{
    public static class ImportSubscriptionHttpTrigger
    {
        [FunctionName(nameof(GetImportSubscriptionById))]
        public static async Task<IActionResult> GetImportSubscriptionById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "ImportSubscriptions/{id}")]
            HttpRequest req,
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
                Imports = Enum.GetNames(typeof(ImportType)),
                LastSuccessfulImport = null,
                NextScheduledImport = DateTime.Now.AddDays(-1)
            });

            return new OkObjectResult(subscription);
        }

        [FunctionName(nameof(GetImportSubscriptionsByPartnerId))]
        public static async Task<IActionResult> GetImportSubscriptionsByPartnerId(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Partners/{partnerId}/ImportSubscriptions")]
            HttpRequest req,
            ILogger log,
            string partnerId)
        {
            var subscriptions = await Task.Run(() => new[]
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
                    Imports = new[]
                    {
                        ImportType.Demographic.ToString()
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
                    Imports = new[]
                    {
                        ImportType.Firmographic.ToString()
                    },
                    LastSuccessfulImport = null,
                    NextScheduledImport = DateTime.Now.AddDays(-1)
                }
            });

            return new OkObjectResult(subscriptions);
        }

        [FunctionName(nameof(CreateImportSubscription))]
        public static async Task<IActionResult> CreateImportSubscription(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ImportSubscriptions")]
            HttpRequest req,
            ILogger log)
        {
            var body = await req.GetModelAsync<ImportSubscription>();
            if (!body.IsValid)
                return new BadRequestObjectResult(body.ValidationMessages);

            // create it

            return new CreatedResult($"ImportSubscriptions/{body.Model.Id}", body.Model);
        }

        [FunctionName(nameof(UpdateImportSubscription))]
        public static async Task<IActionResult> UpdateImportSubscription(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "ImportSubscriptions/{id}")]
            HttpRequest req,
            ILogger log,
            string id)
        {
            var body = await req.GetModelAsync<ImportSubscription>();
            if (!body.IsValid)
                return new BadRequestObjectResult(body.ValidationMessages);

            // update it

            return new OkResult();
        }
    }
}
