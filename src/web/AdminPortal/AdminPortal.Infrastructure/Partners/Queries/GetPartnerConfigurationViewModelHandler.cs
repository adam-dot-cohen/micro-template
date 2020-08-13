using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Partners.Queries;
using Laso.Identity.Api.V1;
using Laso.Mediation;
using Laso.Provisioning.Api.V1;
using Microsoft.Extensions.Hosting;

namespace Laso.AdminPortal.Infrastructure.Partners.Queries
{
    public class GetPartnerConfigurationViewModelHandler : IQueryHandler<GetPartnerConfigurationViewModelQuery, PartnerConfigurationViewModel>
    {
        private readonly Identity.Api.V1.Partners.PartnersClient _partnersClient;
        private readonly Provisioning.Api.V1.Partners.PartnersClient _provisioningClient;
        private readonly IHostEnvironment _hostEnvironment;

        public GetPartnerConfigurationViewModelHandler(
            Identity.Api.V1.Partners.PartnersClient partnersClient, 
            Provisioning.Api.V1.Partners.PartnersClient provisioningClient, IHostEnvironment hostEnvironment)
        {
            _partnersClient = partnersClient;
            _provisioningClient = provisioningClient;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<QueryResponse<PartnerConfigurationViewModel>> Handle(GetPartnerConfigurationViewModelQuery query, CancellationToken cancellationToken)
        {
            var provisionedReply =
                await _provisioningClient.GetPartnerResourcesAsync(new GetPartnerResourcesRequest {PartnerId = query.Id});

            var partnerReply = await _partnersClient.GetPartnerAsync(new GetPartnerRequest { Id = query.Id });

            var model = new PartnerConfigurationViewModel
            {
                Id = query.Id,
                Name = partnerReply.Partner.Name,
                CanDelete = _hostEnvironment.EnvironmentName != "Production",
                Settings = provisionedReply.Resources
                    .Select((r, i) => new PartnerConfigurationViewModel.ConfigurationSetting
                    {
                        Category = r.ResourceType,
                        IsSensitive = r.Sensitive,
                        Name = r.Name,
                        //TODO: check if it's link and format it correctly if it is
                        Value = r.DisplayValue
                    })
                    .ToList()
            };

            return QueryResponse.Succeeded(model);
        }
    }
}
