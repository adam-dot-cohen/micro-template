using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Partner.Api.DataExport;
using Partner.Services.DataExport;

namespace Partner.Api.Functions.DataExport
{
    public class DataExport
    {
        private readonly IDataExporterFactory _factory;

        public DataExport(IDataExporterFactory factory)
        {
            _factory = factory;
        }

        [FunctionName(nameof(DataExport))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            using var sr = new StreamReader(req.Body);

            try
            {
                var body = await sr.ReadToEndAsync();

                log.LogInformation(body);

                var exportRequest = JsonConvert.DeserializeObject<ExportRequest>(body);
                var exporter = _factory.Create(exportRequest.Partner);

                await exporter.ExportAsync(exportRequest.Exports);
            }
            catch (JsonSerializationException ex)
            {
                return new BadRequestObjectResult($"Invalid request: {ex.Message}");
            }                       

            return new OkResult();                
        }
    }
}
