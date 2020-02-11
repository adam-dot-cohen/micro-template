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
    public interface IPartnerService
    {
        Task<Partner> GetAsync(string id);
        Task<IEnumerable<Partner>> GetByInternalIdAsync(PartnerIdentifier internalIdentifier);
        Task<IEnumerable<Partner>> GetAllAsync();
        Task<string> CreateAsync(Partner partner);
        Task UpdateAsync(Partner partner);
        Task DeleteAsync(Partner partner);
        Task DeleteAsync(string id);
    }

    public class PartnerService : IPartnerService
    {
        private readonly IConnectionStringsConfiguration _config;        

        public PartnerService(IConnectionStringsConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));            
        }

        public async Task<Partner> GetAsync(string id)
        {
            return await _config.PartnerServiceBasePath
                .AppendPathSegments(_config.PartnersPath, id)                
                .GetJsonAsync<Partner>();
        }

        public async Task<IEnumerable<Partner>> GetByInternalIdAsync(PartnerIdentifier internalIdentifier)
        {
            return await _config.PartnerServiceBasePath
                .AppendPathSegments(_config.PartnersPath, "search")
                .SetQueryParam("internalId", internalIdentifier)
                .GetJsonAsync<IEnumerable<Partner>>();
        }

        public async Task<IEnumerable<Partner>> GetAllAsync()
        {
            return await _config.PartnerServiceBasePath
                .AppendPathSegments(_config.PartnersPath, "search")                
                .GetJsonAsync<IEnumerable<Partner>>();
        }      

        public async Task<string> CreateAsync(Partner partner)
        {
            var response = await _config.PartnerServiceBasePath
                .AppendPathSegment(_config.PartnersPath)
                .PostJsonAsync(partner)
                .ReceiveJson<dynamic>();

            return response.id;
        }

        public async Task UpdateAsync(Partner partner)
        {
            await _config.PartnerServiceBasePath
               .AppendPathSegments(_config.PartnersPath, partner.Id)
               .PutJsonAsync(partner);
        }

        public async Task DeleteAsync(Partner partner)
        {
            await _config.PartnerServiceBasePath
             .AppendPathSegments(_config.PartnersPath, partner.Id)
             .DeleteAsync();
        }

        public async Task DeleteAsync(string id)
        {
            await _config.PartnerServiceBasePath
             .AppendPathSegments(_config.PartnersPath, id)
             .DeleteAsync();
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
