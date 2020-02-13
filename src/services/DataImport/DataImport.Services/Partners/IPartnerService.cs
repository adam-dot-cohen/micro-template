using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataImport.Core.Configuration;
using DataImport.Services.DTOs;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;

namespace DataImport.Services.Partners
{
    public interface IPartnerService : IServiceClient<string, Partner>
    {        
        Task<IEnumerable<Partner>> GetByInternalIdAsync(PartnerIdentifier internalIdentifier);     
    }

    public class PartnerService : WebServiceClientBase<string, Partner>, IPartnerService
    {
        protected override string ApiBasePath { get; set; }
        protected override string ResourcePath { get; set; }

        public PartnerService(IOptions<RestServiceEndpointConfiguration> config)
        {
            ApiBasePath = config.Value.PartnerServiceBasePath;
            ResourcePath = config.Value.PartnersResourcePath;
        }     

        public async Task<IEnumerable<Partner>> GetByInternalIdAsync(PartnerIdentifier internalIdentifier)
        {
            return await ApiBasePath
                .AppendPathSegments(ResourcePath, SearchPath)
                .SetQueryParam("internalId", internalIdentifier)
                .GetJsonAsync<IEnumerable<Partner>>();
        }    
    }

    public class DummyPartnerService : DymmyServiceClientBase<string, Partner>, IPartnerService
    {
        protected override IEnumerable<Partner> Dtos => new[]
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

        public Task<IEnumerable<Partner>> GetByInternalIdAsync(PartnerIdentifier internalIdentifier)
        {
            return Task.FromResult(Dtos.Where(p => p.InternalIdentifier == internalIdentifier));
        }
    }
}
