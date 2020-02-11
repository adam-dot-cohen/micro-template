using System;
using System.Collections.Generic;
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
}
