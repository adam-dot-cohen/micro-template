using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Mediation.Query;
using Infrastructure.Mediation.Validation;
using Laso.AdminPortal.Core.Partners.Queries;
using Laso.Identity.Api.V1;

namespace Laso.AdminPortal.Infrastructure.Partners.Queries
{
    public class GetPartnerProvisioningHistoryViewModelHandler : IQueryHandler<GetPartnerProvisioningHistoryViewModelQuery,PartnerProvisioningHistoryViewModel>
    {
        private readonly Identity.Api.V1.Partners.PartnersClient _partnersClient;
        private readonly Provisioning.Api.V1.Partners.PartnersClient _provisioningClient;

        public GetPartnerProvisioningHistoryViewModelHandler(Identity.Api.V1.Partners.PartnersClient partnersClient, Provisioning.Api.V1.Partners.PartnersClient provisioningClient)
        {
            _partnersClient = partnersClient;
            _provisioningClient = provisioningClient;
        }

        public async Task<QueryResponse<PartnerProvisioningHistoryViewModel>> Handle(GetPartnerProvisioningHistoryViewModelQuery query, CancellationToken cancellationToken)
        {
            var partnerReply = await _partnersClient.GetPartnerAsync(new GetPartnerRequest { Id = query.Id });

            if (partnerReply.Partner == null || string.IsNullOrWhiteSpace(partnerReply.Partner.Name))
                return QueryResponse.Failed<PartnerProvisioningHistoryViewModel>(new[] {new ValidationMessage("Partner",$"{query.Id} was not found.")});

            var provisioningReply =
                await _provisioningClient.GetPartnerHistoryAsync(new Provisioning.Api.V1.GetPartnerHistoryRequest
                    {PartnerId = query.Id});

            if (string.IsNullOrWhiteSpace(provisioningReply.PartnerId))
                return QueryResponse.Failed<PartnerProvisioningHistoryViewModel>(new[]
                    {new ValidationMessage("ProvisioningHistory", $"No provisioning history found for {query.Id}")});

            var model = new PartnerProvisioningHistoryViewModel
            {
                Id = query.Id,
                Name = partnerReply.Partner.Name,
                History = provisioningReply.Events.Select(e => new PartnerProvisioningEventViewModel
                {
                    Sequence = e.Sequence,
                    EventType = e.EventType,
                    Started = e.Started,
                    Completed = e.Completed,
                    Succeeded = e.Succeeded,
                    Errors = e.ErrorMsg
                }).ToList()
            };
            model.History.OrderByDescending(h => h.Sequence);

            return QueryResponse.Succeeded(model);
        }
    }
}
