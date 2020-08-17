using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Laso.Provisioning.Api.V1;
using Laso.Provisioning.Core;
using Laso.Provisioning.Core.Extensions;
using Laso.TableStorage;
using Microsoft.AspNetCore.Authorization;
using Provisioning.Domain.Entities;

namespace Laso.Provisioning.Api.Services
{
    [Authorize]
    public class PartnersServiceV1 : Partners.PartnersBase
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly ISubscriptionProvisioningService _provisioningService;
        private readonly IResourceLocator _resourceLocator;

        public PartnersServiceV1(ITableStorageService tableStorageService, ISubscriptionProvisioningService provisioningService, IResourceLocator resourceLocator)
        {
            _tableStorageService = tableStorageService;
            _provisioningService = provisioningService;
            _resourceLocator = resourceLocator;
        }

        public override async Task<ProvisionPartnerReply> ProvisionPartner(ProvisionPartnerRequest request,
            ServerCallContext callContext)
        {
            await _provisioningService.ProvisionPartner(request.PartnerId, request.PartnerName,
                callContext.CancellationToken);
            return new ProvisionPartnerReply {PartnerId = request.PartnerId};
        }

        public override async Task<RemovePartnerReply> RemovePartner(RemovePartnerRequest request,
            ServerCallContext callContext)
        {
            await _provisioningService.RemovePartner(request.PartnerId, callContext.CancellationToken);
            return new RemovePartnerReply {PartnerId = request.PartnerId};
        }

        public override async Task<GetPartnerHistoryReply> GetPartnerHistory(GetPartnerHistoryRequest request,
            ServerCallContext serverCallContext)
        {
            var history = new List<ProvisioningEventView>();
            //TODO: add paging
            var actionEvents = (await _tableStorageService.GetAllAsync<ProvisioningActionEvent>(request.PartnerId)).OrderByDescending(x => x.Completed);

            var sequence = 1;  //TODO: there must be a better way to do this . . . 
            actionEvents.ForEach(ae =>
            {
                history.Add(new ProvisioningEventView
                {
                    PartnerId = ae.PartnerId,
                    EventType = ae.ToString(),
                    Sequence = sequence, //using a numeric sequence to avoid making the client parse the datetime strings
                    Succeeded = ae.Succeeded,
                    Completed = ae.Completed.ToString("G"),
                    ErrorMsg = ae.ErrorMessage ?? string.Empty
                });
                sequence++;
            });

            var reply = new GetPartnerHistoryReply { PartnerId = request.PartnerId };
            reply.Events.AddRange(history);

            return reply;
        }

        public override async Task<GetPartnerResourcesReply> GetPartnerResources(GetPartnerResourcesRequest request,
            ServerCallContext serverCallContext)
        {
            var reply = new GetPartnerResourcesReply {PartnerId = request.PartnerId};

            var resourceEvents = (await _tableStorageService.GetAllAsync<ProvisionedResourceEvent>(request.PartnerId))
                .AsQueryable()
                .OrderByDescending(re => re.ProvisionedOn)
                .ToList();

            var resourceViews = new List<PartnerResourceView>();

            foreach (var resourceEvent in resourceEvents)
            {
                var value = await _resourceLocator.GetLocationString(resourceEvent);
                resourceViews.Add(new PartnerResourceView
                {
                    ResourceType = resourceEvent.Type.ToString(),
                    Name = resourceEvent.DisplayName,
                    Link = false,
                    Sensitive = resourceEvent.Sensitive,
                    DisplayValue = value
                });
            }

            reply.Resources.AddRange(resourceViews);

            return reply;
        }
    }
}