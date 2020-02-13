using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using DataImport.Core.Configuration;
using System.Linq;
using DataImport.Services.DTOs;
using Microsoft.Extensions.Options;

namespace DataImport.Services
{
    public interface IImportSubscriptionsService : IServiceClient<string, ImportSubscriptionDto>
    {
        Task<IEnumerable<ImportSubscriptionDto>> GetByPartnerIdAsync(string partnerId);
    }

    public class ImportSubscriptionsService : WebServiceClientBase<string, ImportSubscriptionDto>, IImportSubscriptionsService
    {
        protected override string ApiBasePath { get; set; }
        protected override string ResourcePath { get; set; }

        public ImportSubscriptionsService(IOptions<RestServiceEndpointConfiguration> config)
        {
            ApiBasePath = config.Value.SubscriptionsServiceBasePath;
            ResourcePath = config.Value.SubscriptionsResourcePath;
        }

        public async Task<IEnumerable<ImportSubscriptionDto>> GetByPartnerIdAsync(string partnerId)
        {
            return await ApiBasePath
                .AppendPathSegments(ResourcePath, SearchPath)
                .SetQueryParam("partnerId", partnerId)
                .GetJsonAsync<IEnumerable<ImportSubscriptionDto>>();
        }
    }

    public class DummyImportSubscriptionsService : DymmyServiceClientBase<string, ImportSubscriptionDto>, IImportSubscriptionsService
    {
        protected override IEnumerable<ImportSubscriptionDto> Dtos => new[]
        {            
            new ImportSubscriptionDto
            {
                Id = "1",
                PartnerId = "2",
                Frequency = ImportFrequencyDto.Weekly,
                IncomingStorageLocation = "partner-Quarterspot/incoming",
                EncryptionType = EncryptionTypeDto.PGP,
                OutputFileType = FileTypeDto.CSV,
                Imports = new []
                {
                    ImportTypeDto.Demographic,
                    ImportTypeDto.Firmographic
                }
            }
        };      

        public Task<IEnumerable<ImportSubscriptionDto>> GetByPartnerIdAsync(string partnerId)
        {
            return Task.FromResult(Dtos.Where(s => s.PartnerId == partnerId));
        }
    }
}
