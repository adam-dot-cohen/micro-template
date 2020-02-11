using System.IO;
using System.Threading.Tasks;
using DataImport.Domain.Api;
using DataImport.Services.DataImport;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DataImport.Api.Functions.Import
{
    public class ImportController
    {
        private readonly IDataImporterFactory _factory;

        public ImportController(IDataImporterFactory factory)
        {
            _factory = factory;
        }

        [FunctionName(nameof(BeginImport))]
        public async Task<IActionResult> BeginImport(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "BeginImport")] HttpRequest req,
            ILogger log)
        {
            try
            {                
                using var sr = new StreamReader(req.Body);
                var body = await sr.ReadToEndAsync();
                
                var importReq = JsonConvert.DeserializeObject<ImportRequest>(body);
                var importer = _factory.Create(importReq.Partner);

                await importer.ImportAsync(importReq.Imports);
            }
            catch (JsonSerializationException ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult($"Invalid request: {ex.Message}");
            }

            return new OkResult();                
        }
    }
}
