using System.IO;
using System.Threading.Tasks;
using DataImport.Domain.Api;
using DataImport.Services.Imports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DataImport.Services.Partners;
using System;
using System.Collections.Generic;
using System.Linq;
using DataImport.Services.SubscriptionHistory;
using DataImport.Services.Subscriptions;

namespace DataImport.Api.Functions.Import
{
    public class ImportsController
    {        
        private readonly IDataImporterFactory _factory;
        private readonly IPartnerService _partnerService;
        private readonly IImportSubscriptionsService _subscriptionsService;
        private readonly IImportHistoryService _importHistoryService;

        public ImportsController(
            IDataImporterFactory factory, 
            IPartnerService partnerService,
            IImportSubscriptionsService subscriptionsService,
            IImportHistoryService importHistoryService)
        {
            _factory = factory;
            _partnerService = partnerService;
            _subscriptionsService = subscriptionsService;
            _importHistoryService = importHistoryService;
        }       

        [FunctionName(nameof(BeginImport))]
        public async Task<IActionResult> BeginImport(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "imports/BeginImport")] HttpRequest req,
            ILogger log)
        {
            try
            {
                using var sr = new StreamReader(req.Body);
                var body = await sr.ReadToEndAsync();                               
                
                var request = JsonConvert.DeserializeObject<ImportRequest>(body);
                var partner = await _partnerService.GetAsync(request.PartnerId);
                var importer = _factory.Create(partner.InternalIdentifier);
                var subscriptions = await _subscriptionsService.GetByPartnerIdAsync(request.PartnerId);

                foreach (var sub in subscriptions)
                {
                    string[] failReasons = null;

                    try
                    {
                        await importer.ImportAsync(sub);
                    }
                    catch (AggregateException ex)
                    {
                        failReasons = ex.InnerExceptions.Select(e => e.Message).ToArray();
                    }

                    await _importHistoryService.CreateAsync(new ImportHistory
                    {
                        Completed = DateTime.UtcNow,
                        SubscriptionId = sub.Id,
                        Success = failReasons.Any(),
                        FailReasons = failReasons
                    });
                }
            }
            catch (JsonSerializationException ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult($"Invalid request: {ex.Message}");
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }

            return new OkResult();
        }
    }
}
