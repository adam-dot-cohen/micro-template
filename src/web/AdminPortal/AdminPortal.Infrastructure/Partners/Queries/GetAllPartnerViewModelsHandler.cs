using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Mediation.Query;
using Laso.AdminPortal.Core.Partners.Queries;
using Laso.Identity.Api.V1;

namespace Laso.AdminPortal.Infrastructure.Partners.Queries
{
    public class GetAllPartnerViewModelsHandler : QueryHandler<GetAllPartnerViewModelsQuery, IReadOnlyCollection<PartnerViewModel>>
    {
        private readonly Identity.Api.V1.Partners.PartnersClient _partnersClient;

        public GetAllPartnerViewModelsHandler(Identity.Api.V1.Partners.PartnersClient partnersClient)
        {
            _partnersClient = partnersClient;
        }

        public override async Task<QueryResponse<IReadOnlyCollection<PartnerViewModel>>> Handle(GetAllPartnerViewModelsQuery query, CancellationToken cancellationToken)
        {
            var reply = await _partnersClient.GetPartnersAsync(new GetPartnersRequest(), cancellationToken: cancellationToken);

            var model = reply.Partners
                .Select(partner => new PartnerViewModel
                {
                    Id = partner.Id,
                    Name = partner.Name,
                    ContactName = partner.ContactName,
                    ContactPhone = partner.ContactPhone,
                    ContactEmail = partner.ContactEmail
                })
                .OrderBy(p => p.Name)
                .ToList();

            return Succeeded(model);
        }
    }
}