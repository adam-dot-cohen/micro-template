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
    public interface IPartnerService : IServiceClient<string, PartnerDto>
    {        
        Task<IEnumerable<PartnerDto>> GetByInternalIdAsync(PartnerIdentifierDto internalIdentifier);     
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

        public async Task<IEnumerable<PartnerDto>> GetByInternalIdAsync(PartnerIdentifierDto internalIdentifier)
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
                InternalIdentifier = PartnerIdentifierDto.Laso,
                Name = "LASO"
            },
            new PartnerDto
            {
                Id = "2",
                InternalIdentifier = PartnerIdentifierDto.Quarterspot,
                Name = "Quarterspot"
            }
        };       

        public Task<IEnumerable<PartnerDto>> GetByInternalIdAsync(PartnerIdentifierDto internalIdentifier)
        {
            return Task.FromResult(Dtos.Where(p => p.InternalIdentifier == internalIdentifier));
        }
    }
}
