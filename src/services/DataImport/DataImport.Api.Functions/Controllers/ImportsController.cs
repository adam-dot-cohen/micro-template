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
using System.Collections.Generic;
using System.Linq;
using DataImport.Api.Extensions;
using System;

namespace DataImport.Api.Functions.Import
{
    public class ImportsController
    {        
        private readonly IDataImporterFactory _factory;

        public ImportsController(IDataImporterFactory factory)
        {
            _factory = factory;
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
                
                // todo: get the subscription info
                // todo: add import history

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
