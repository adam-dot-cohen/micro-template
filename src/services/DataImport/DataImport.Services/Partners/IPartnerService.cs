using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DataImport.Core.Configuration;
using DataImport.Domain.Api;
using Flurl;
using Flurl.Http;

namespace DataImport.Services.Partners
{
    public interface IPartnerService : IServiceClient<string, Partner>
    {        
        Task<IEnumerable<Partner>> GetByInternalIdAsync(PartnerIdentifier internalIdentifier);     
    }

    public class PartnerService : ServiceClientBase<string, Partner>, IPartnerService
    {
        protected override string ApiBasePath { get; set; }
        protected override string ResourcePath { get; set; }

        public PartnerService(IConnectionStringsConfiguration config)
        {
            ApiBasePath = config.PartnerServiceBasePath;
            ResourcePath = config.PartnersResourcePath;
        }     

        public async Task<IEnumerable<Partner>> GetByInternalIdAsync(PartnerIdentifier internalIdentifier)
        {
            return await ApiBasePath
                .AppendPathSegments(ResourcePath, SearchPath)
                .SetQueryParam("internalId", internalIdentifier)
                .GetJsonAsync<IEnumerable<Partner>>();
        }    
    }

    public class DummyPartnerService : IPartnerService
    {
        private static readonly IEnumerable<Partner> Partners = new[]
        {
            new Partner
            {
                Id = "1",
                InternalIdentifier = PartnerIdentifier.Laso,
                Name = "LASO"
            },
            new Partner
            {
                Id = "2",
                InternalIdentifier = PartnerIdentifier.Quarterspot,
                Name = "Quarterspot"
            }
        };

        public Task<string> CreateAsync(Partner partner)
        {
            return Task.FromResult<string>(null);
        }

        public Task DeleteAsync(Partner partner)
        {
            return Task.FromResult<object>(null);
        }

        public Task DeleteAsync(string id)
        {
            return Task.FromResult<object>(null);
        }

        public Task<IEnumerable<Partner>> GetAllAsync()
        {
            return Task.FromResult(Partners);
        }

        public Task<Partner> GetAsync(string id)
        {
            var partner = Partners.SingleOrDefault(p => p.Id == id);
            if (partner == null)
                throw new HttpRequestException($"404 Partner {id} not found");

            return Task.FromResult(partner);
        }

        public Task<IEnumerable<Partner>> GetByInternalIdAsync(PartnerIdentifier internalIdentifier)
        {
            return Task.FromResult(Partners.Where(p => p.InternalIdentifier == internalIdentifier));
        }

        public Task UpdateAsync(Partner partner)
        {
            return Task.FromResult<object>(null);
        }
    }
}
