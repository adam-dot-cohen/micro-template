using DataImport.Domain.Api;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using DataImport.Core.Configuration;
using System.Linq;

namespace DataImport.Services.Subscriptions
{
    public interface IImportSubscriptionsService : IServiceClient<string, ImportSubscription>
    {
        Task<IEnumerable<ImportSubscription>> GetByPartnerIdAsync(string partnerId);
    }

    public class ImportSubscriptionsService : WebServiceClientBase<string, ImportSubscription>, IImportSubscriptionsService
    {
        protected override string ApiBasePath { get; set; }
        protected override string ResourcePath { get; set; }

        public ImportSubscriptionsService(IConnectionStringsConfiguration config)
        {
            ApiBasePath = config.SubscriptionsServiceBasePath;
            ResourcePath = config.SubscriptionsResourcePath;
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
        protected override IEnumerable<ImportSubscription> DummyCollection => new[]
        {            
            new ImportSubscription
            {
                Id = "1",
                PartnerId = "2",
                Frequency = ImportFrequency.Weekly.ToString(),
                IncomingStorageLocation = "partner-Quarterspot/incoming",
                OutgoingStorageLocation = "partner-Quarterspot/outgoing",
                EncryptionType = EncryptionType.PGP,
                OutputFileType = FileType.CSV,
                Imports = new []
                {
                    ImportType.Demographic,
                    ImportType.Firmographic
                }
                .Select(e => e.ToString()).ToArray()
            }
        };      

        public Task<IEnumerable<ImportSubscription>> GetByPartnerIdAsync(string partnerId)
        {
            return Task.FromResult(DummyCollection.Where(s => s.PartnerId == partnerId));
        }
    }
}
