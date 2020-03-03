using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Laso.DataImport.Core.Configuration;
using Flurl;
using Flurl.Http;
using Laso.DataImport.Services.DTOs;
using Microsoft.Extensions.Options;

namespace Laso.DataImport.Services
{
    public interface IPartnerService : IServiceClient<string, PartnerDto>
    {        
        Task<IEnumerable<PartnerDto>> GetByInternalIdAsync(PartnerIdentifier internalIdentifier);     
    }

    public class PartnerService : WebServiceClientBase<string, PartnerDto>, IPartnerService
    {
        protected override string ApiBasePath { get; set; }
        protected override string ResourcePath { get; set; }

        public PartnerService(IOptions<RestServiceEndpointConfiguration> config)
        {
            ApiBasePath = config.Value.PartnerServiceBasePath;
            ResourcePath = config.Value.PartnersResourcePath;
        }     

        public async Task<IEnumerable<PartnerDto>> GetByInternalIdAsync(PartnerIdentifier internalIdentifier)
        {
            return await ApiBasePath
                .AppendPathSegments(ResourcePath, SearchPath)
                .SetQueryParam("internalId", internalIdentifier)
                .GetJsonAsync<IEnumerable<PartnerDto>>();
        }    
    }

    public class DummyPartnerService : DymmyServiceClientBase<string, PartnerDto>, IPartnerService
    {
        protected override IEnumerable<PartnerDto> Dtos => new[]
        {
            new PartnerDto
            {
                Id = "1",
                InternalIdentifier = PartnerIdentifier.Laso,
                Name = "LASO"
            },
            new PartnerDto
            {
                Id = "2",
                InternalIdentifier = PartnerIdentifier.Quarterspot,
                Name = "Quarterspot"
            }
        };       

        public Task<IEnumerable<PartnerDto>> GetByInternalIdAsync(PartnerIdentifier internalIdentifier)
        {
            return Task.FromResult(Dtos.Where(p => p.InternalIdentifier == internalIdentifier));
        }
    }
}
