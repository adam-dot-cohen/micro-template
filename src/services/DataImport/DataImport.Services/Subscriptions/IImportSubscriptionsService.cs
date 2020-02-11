using DataImport.Domain.Api;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using DataImport.Core.Configuration;

namespace DataImport.Services.Subscriptions
{
    public interface IImportSubscriptionsService : IServiceClient<string, ImportSubscription>
    {
        Task<IEnumerable<ImportSubscription>> GetByPartnerIdAsync(string partnerId);
    }

    public class ImportSubscriptionsService : ServiceClientBase<string, ImportSubscription>, IImportSubscriptionsService
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

    public class DummyImportSubscriptionsService : IImportSubscriptionsService
    {
        public Task<string> CreateAsync(ImportSubscription dto)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(ImportSubscription dto)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ImportSubscription>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ImportSubscription> GetAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ImportSubscription>> GetByPartnerIdAsync(string partnerId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(ImportSubscription dto)
        {
            throw new NotImplementedException();
        }
    }
}
