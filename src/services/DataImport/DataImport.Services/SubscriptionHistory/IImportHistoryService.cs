using System;
using System.Threading.Tasks;
using DataImport.Core.Configuration;
using DataImport.Services.DTOs;
using Microsoft.Extensions.Options;

namespace DataImport.Services.SubscriptionHistory
{
    public interface IImportHistoryService : IServiceClient<string, ImportHistory>
    {
    }

    public class ImportHistoryService : WebServiceClientBase<string, ImportHistory>, IImportHistoryService
    {
        protected override string ApiBasePath { get; set; }
        protected override string ResourcePath { get; set; }

        public ImportHistoryService(IOptions<RestServiceEndpointConfiguration> config)
        {
            ApiBasePath = config.Value.ImportHistoryServiceBasePath;
            ResourcePath = config.Value.ImportHistoryResourcePath;
        }

        public override Task UpdateAsync(ImportHistory dto)
        {
            throw new NotSupportedException();
        }

        public override Task DeleteAsync(ImportHistory dto)
        {
            throw new NotSupportedException();
        }

        public override Task DeleteAsync(string id)
        {
            throw new NotSupportedException();
        }
    }
}
