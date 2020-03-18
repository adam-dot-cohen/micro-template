using System.Threading;
using System.Threading.Tasks;
using Identity.Api.V1;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Core.Partners.Queries;

namespace Laso.AdminPortal.Infrastructure.Partners.Queries
{
    public class GetPartnerViewModelHandler : IQueryHandler<GetPartnerViewModelQuery, PartnerViewModel>
    {
        private readonly Identity.Api.V1.Partners.PartnersClient _partnersClient;

        public GetPartnerViewModelHandler(Identity.Api.V1.Partners.PartnersClient partnersClient)
        {
            _partnersClient = partnersClient;
        }

        public async Task<QueryResponse<PartnerViewModel>> Handle(GetPartnerViewModelQuery query, CancellationToken cancellationToken)
        {
            var reply = await _partnersClient.GetPartnerAsync(new GetPartnerRequest { Id = query.PartnerId }, cancellationToken: cancellationToken);

            var model = new PartnerViewModel
            {
                Id = reply.Partner.Id,
                Name = reply.Partner.Name,
                ContactName = reply.Partner.ContactName,
                ContactPhone = reply.Partner.ContactPhone,
                ContactEmail = reply.Partner.ContactEmail
            };

            return QueryResponse.Succeeded(model);
        }
    }
}