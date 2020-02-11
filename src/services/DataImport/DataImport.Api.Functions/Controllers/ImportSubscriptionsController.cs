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
using System.Collections.Generic;
using System.Linq;

namespace DataImport.Api
{
    public static class ImportSubscriptionsController
    {
        private static readonly List<ImportSubscription> Subscriptions = new List<ImportSubscription>
        {
            new ImportSubscription
            {
                Id = "1",
                PartnerId = "2",
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
                PartnerId = "2",
                Frequency = ImportFrequency.Weekly.ToString(),
                Imports = new[]
                {
                    ImportType.Firmographic.ToString()
                },
                LastSuccessfulImport = null,
                NextScheduledImport = DateTime.Now.AddDays(-1)
            }
        };

        [FunctionName(nameof(Get))]
        public static async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "subscriptions/{id:int}")]
            HttpRequest req,
            ILogger log,
            int id)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var sub = Subscriptions.SingleOrDefault(s => s.Id == id.ToString());
                if (sub == null)
                    return new NotFoundResult();

                return new OkObjectResult(sub);
            });
        }

        [FunctionName(nameof(Search))]
        public static async Task<IActionResult> Search(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "subscriptions/search")]
            HttpRequest req,
            ILogger log)
        {
            if (!req.Query.Any())
                return new OkObjectResult(Subscriptions);

            var predicates = new Func<ImportSubscription, bool>[]
            {
                s => s.PartnerId == req.Query["partnerId"]
            };

            var filtered = Subscriptions.Where(s => predicates.All(p => p(s)));
            return await Task.Run(() => new OkObjectResult(filtered));
        }

        [FunctionName(nameof(Post))]
        public static async Task<IActionResult> Post(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "subscriptions")]
            HttpRequest req,
            ILogger log)
        {
            var body = await req.GetModelAsync<ImportSubscription>();
            if (!body.IsValid)
                return new BadRequestObjectResult(body.ValidationMessages);

            if (body.Model.Id != null && Subscriptions.Any(s => s.Id == body.Model.Id))
                return new ConflictObjectResult($"Subscription {body.Model.Id} already exists");

            body.Model.Id = (Subscriptions.Select(s => int.Parse(s.Id)).Max() + 1).ToString();

            Subscriptions.Add(body.Model);

            return new CreatedResult($"imports/subscriptions/{body.Model.Id}", body.Model);
            //return new CreatedAtRouteResult("imports/subscriptions", new { body.Model.Id }, body.Model);
        }

        [FunctionName(nameof(Put))]
        public static async Task<IActionResult> Put(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "subscriptions/{id:int}")]
            HttpRequest req,
            ILogger log,
            int id)
        {
            var body = await req.GetModelAsync<ImportSubscription>();
            if (!body.IsValid)
                return new BadRequestObjectResult(body.ValidationMessages);

            var index = Subscriptions.FindIndex(s => s.Id == id.ToString());
            if (index == -1)
                return new NotFoundObjectResult($"Subscription {id} does not exist");

            Subscriptions[index] = body.Model;

            return new OkResult();
        }

        [FunctionName(nameof(Delete))]
        public static async Task<IActionResult> Delete(
          [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "subscriptions/{id:int}")] HttpRequest req,
          ILogger log,
          int id)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var existing = Subscriptions.SingleOrDefault(s => s.Id == id.ToString());
                if (existing == null)
                    return new NotFoundObjectResult($"Subscription {id} does not exist");

                Subscriptions.Remove(existing);

                return new OkResult();
            });
        }
    }
}
