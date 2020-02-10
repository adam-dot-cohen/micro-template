using System;
using System.Threading.Tasks;
using DataImport.Domain;
using DataImport.Api.Extensions;
using DataImport.Domain.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DataImport.Api
{
    public static class ImportSubscriptionsController
    {
        [FunctionName(nameof(Get))]
        public static async Task<IActionResult> Get(
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
                PartnerId = "1",
                Frequency = ImportFrequency.Weekly.ToString(),
                Imports = Enum.GetNames(typeof(ImportType)),
                LastSuccessfulImport = null,
                NextScheduledImport = DateTime.Now.AddDays(-1)
            });

            return new OkObjectResult(subscription);
        }

        [FunctionName(nameof(GetByPartnerId))]
        public static async Task<IActionResult> GetByPartnerId(
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
                    PartnerId = partnerId,
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
                    PartnerId = partnerId,
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

        [FunctionName(nameof(Post))]
        public static async Task<IActionResult> Post(
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

        [FunctionName(nameof(Put))]
        public static async Task<IActionResult> Put(
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

        [FunctionName(nameof(Delete))]
        public static async Task<IActionResult> Delete(
          [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "ImportSubscriptions/{id}")] HttpRequest req,
          ILogger log,
          string id)
        {        
            return await Task.Run<IActionResult>(() => new OkResult());
        }
    }
}
