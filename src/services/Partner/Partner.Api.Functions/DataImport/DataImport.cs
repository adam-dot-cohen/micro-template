using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Partner.Api.DataImport;
using Partner.Services.DataImport;

namespace Partner.Api.Functions.DataImport
{
    public class DataImport
    {
        private readonly IDataImporterFactory _factory;

        public DataImport(IDataImporterFactory factory)
        {
            _factory = factory;
        }

        [FunctionName(nameof(DataImport))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {                
                using var sr = new StreamReader(req.Body);
                var body = await sr.ReadToEndAsync();
                
                var importReq = JsonConvert.DeserializeObject<ImportRequest>(body);
                var importer = _factory.Create(importReq.ExportFrom, importReq.ImportTo);

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
