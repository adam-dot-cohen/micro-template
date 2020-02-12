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
using DataImport.Services.Subscriptions;

namespace DataImport.Api.Functions.Import
{
    public class ImportsController
    {        
        private readonly IDataImporterFactory _factory;
        private readonly IPartnerService _partnerService;
        private readonly IImportSubscriptionsService _subscriptionsService;

        public ImportsController(
            IDataImporterFactory factory, 
            IPartnerService partnerService,
            IImportSubscriptionsService subscriptionsService)
        {
            _factory = factory;
            _partnerService = partnerService;
            _subscriptionsService = subscriptionsService;
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
                    await importer.ImportAsync(sub);

                    // todo: add import history
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
