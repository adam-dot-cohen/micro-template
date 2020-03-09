using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Laso.DataImport.Core.Configuration;
using System.Linq;
using Laso.DataImport.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Laso.DataImport.Services
{
    public interface IImportSubscriptionsService : IServiceClient<string, ImportSubscription>
    {
        Task<IEnumerable<ImportSubscription>> GetByPartnerIdAsync(string partnerId);
    }

    public class ImportSubscriptionsService : WebServiceClientBase<string, ImportSubscription>, IImportSubscriptionsService
    {
        protected override string ApiBasePath { get; set; }
        protected override string ResourcePath { get; set; }

        public ImportSubscriptionsService(IOptions<RestServiceEndpointConfiguration> config)
        {
            ApiBasePath = config.Value.SubscriptionsServiceBasePath;
            ResourcePath = config.Value.SubscriptionsResourcePath;
        }

        public async Task<IEnumerable<ImportSubscription>> GetByPartnerIdAsync(string partnerId)
        {
            return await ApiBasePath
                .AppendPathSegments(ResourcePath, SearchPath)
                .SetQueryParam("partnerId", partnerId)
                .GetJsonAsync<IEnumerable<ImportSubscription>>();
        }
    }

    public class DummyImportSubscriptionsService : DymmyServiceClientBase<string, ImportSubscription>, IImportSubscriptionsService
    {
        protected override IEnumerable<ImportSubscription> Entities => new[]
        {            
            new ImportSubscription
            {
                PartnerId = "2",
                Frequency = ImportFrequency.Weekly,
                IncomingStorageLocation = "partner-Quarterspot/incoming",
                EncryptionType = EncryptionType.Pgp,
                OutputFileType = FileType.Csv,
                Imports = new []
                {
                    ImportType.Demographic,
                    ImportType.Firmographic
                }
            }
        };      

        public Task<IEnumerable<ImportSubscription>> GetByPartnerIdAsync(string partnerId)
        {
            return Task.FromResult(Entities.Where(s => s.PartnerId == partnerId));
        }
    }
}
