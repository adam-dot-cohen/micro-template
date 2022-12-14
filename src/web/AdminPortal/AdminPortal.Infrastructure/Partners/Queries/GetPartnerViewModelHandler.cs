using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Mediation.Query;
using Laso.AdminPortal.Core.Partners.Queries;
using Laso.Identity.Api.V1;

namespace Laso.AdminPortal.Infrastructure.Partners.Queries
{
    public class GetPartnerViewModelHandler : QueryHandler<GetPartnerViewModelQuery, PartnerViewModel>
    {
        private readonly Identity.Api.V1.Partners.PartnersClient _partnersClient;

        public GetPartnerViewModelHandler(Identity.Api.V1.Partners.PartnersClient partnersClient)
        {
            _partnersClient = partnersClient;
        }

        public override async Task<QueryResponse<PartnerViewModel>> Handle(GetPartnerViewModelQuery query, CancellationToken cancellationToken)
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

            return Succeeded(model);
        }
    }
}