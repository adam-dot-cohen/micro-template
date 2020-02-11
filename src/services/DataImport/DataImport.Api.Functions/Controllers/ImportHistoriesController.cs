using System.IO;
using System.Threading.Tasks;
using DataImport.Domain.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using DataImport.Api.Extensions;
using System;

namespace DataImport.Api.Functions.Import
{
    public class ImportHistoriesController
    {
        private static readonly List<ImportHistory> ImportsHistory = new List<ImportHistory>();
      
        [FunctionName(nameof(ImportHistoriesGet))]
        public async Task<IActionResult> ImportHistoriesGet(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = "ImportHistories/{id:int}")] HttpRequest req,
          ILogger log,
          int id)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var import = ImportsHistory.SingleOrDefault(i => i.Id == id.ToString());
                if (import == null)
                    return new NotFoundObjectResult($"Import {id} does not exist");

                return new OkObjectResult(import);
            });
        }

        [FunctionName(nameof(ImportHistoriesSearch))]
        public static async Task<IActionResult> ImportHistoriesSearch(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "ImportHistories/search")]
            HttpRequest req,
        ILogger log)
        {
            if (!req.Query.Any())
                return new OkObjectResult(ImportsHistory);

            var predicates = new Func<ImportHistory, bool>[]
            {
                s => req.Query["subscriptionId"].Any() ? s.SubscriptionId == req.Query["subscriptionId"] : true,
                s =>
                {
                    static DateTime? TryParse(string text) => DateTime.TryParse(text, out var date) ? date : (DateTime?) null;
                    var after = TryParse(req.Query["after"]);
                    var before = TryParse(req.Query["before"]);

                    return s.Completed > (after ?? DateTime.MinValue) && s.Completed < (before ?? DateTime.MaxValue);
                }
            };

            var filtered = ImportsHistory.Where(s => predicates.All(p => p(s))).ToList();
            return await Task.Run(() => new OkObjectResult(filtered));
        }

        [FunctionName(nameof(ImportHistoriesPost))]
        public async Task<IActionResult> ImportHistoriesPost(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ImportHistories")] HttpRequest req,
         ILogger log)
        {
            var body = await req.GetModelAsync<ImportHistory>();
            if (!body.IsValid)
                return new BadRequestObjectResult(body.ValidationMessages);

            if (body.Model.Id != null && ImportsHistory.Any(s => s.Id == body.Model.Id))
                return new ConflictObjectResult($"Import {body.Model.Id} already exists");

            body.Model.Id = ImportsHistory.Any() ? (ImportsHistory.Select(s => int.Parse(s.Id)).Max() + 1).ToString() : "1";

            ImportsHistory.Add(body.Model);

            return new CreatedResult($"ImportHistories/{body.Model.Id}", new { body.Model.Id });
        }       
    }
}
