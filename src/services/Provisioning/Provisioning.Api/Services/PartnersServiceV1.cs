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
            var actionEvents = (await _tableStorageService.GetAllAsync<ProvisioningActionEvent>(request.PartnerId)).OrderByDescending(x => x.Started);

            var sequence = 1;  //TODO: there must be a better way to do this . . . 
            actionEvents.ForEach(ae =>
            {
                history.Add(new ProvisioningEventView
                {
                    PartnerId = ae.PartnerId,
                    EventType = ae.ToString(),
                    Started = ae.Started.ToString("G"),
                    Sequence = sequence, //using a numeric sequence to avoid making the client parse the datetime strings
                    Succeeded = ae.Succeeded,
                    Completed = ae.Succeeded ? ae.Completed.ToString("G") : string.Empty,
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

            var resourceEvents = await _tableStorageService.GetAllAsync<ProvisionedResourceEvent>(request.PartnerId)
                .ContinueWith(ret => ret.Result.AsQueryable().OrderByDescending(re => re.ProvisionedOn).ToList());
            var resourceViews = new List<PartnerResourceView>();
            var tasks = resourceEvents.Select(re =>
                _resourceLocator.GetLocationString(re).ContinueWith(ls =>
                    resourceViews.Add(
                        new PartnerResourceView
                        {
                            ResourceType = re.Type.ToString(),
                            Name = re.DisplayName,
                            Link = false,
                            Sensitive = re.Sensitive,
                            DisplayValue = ls.Result
                        })));
            var waitForThem = Task.WhenAll(tasks);
            waitForThem.Wait(serverCallContext.CancellationToken);

            reply.Resources.AddRange(resourceViews);

            return reply;
        }
    }
}