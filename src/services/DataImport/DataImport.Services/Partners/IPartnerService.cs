using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Laso.DataImport.Core.Configuration;
using Flurl;
using Flurl.Http;
using Laso.DataImport.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Laso.DataImport.Services
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
        protected override IEnumerable<Partner> Entities => new[]
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
            return Task.FromResult(Entities.Where(p => p.InternalIdentifier == internalIdentifier));
        }
    }
}
