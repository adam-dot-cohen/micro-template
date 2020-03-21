using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Identity.Api.V1;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Core.Partners.Queries;

namespace Laso.AdminPortal.Infrastructure.Partners.Queries
{
    public class GetAllPartnerViewModelsHandler : IQueryHandler<GetAllPartnerViewModelsQuery, IReadOnlyCollection<PartnerViewModel>>
    {
        private readonly Identity.Api.V1.Partners.PartnersClient _partnersClient;

        public GetAllPartnerViewModelsHandler(Identity.Api.V1.Partners.PartnersClient partnersClient)
        {
            _partnersClient = partnersClient;
        }

        public async Task<QueryResponse<IReadOnlyCollection<PartnerViewModel>>> Handle(GetAllPartnerViewModelsQuery query, CancellationToken cancellationToken)
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
                }).ToList();

            return QueryResponse.Succeeded<IReadOnlyCollection<PartnerViewModel>>(model);
        }
    }
}