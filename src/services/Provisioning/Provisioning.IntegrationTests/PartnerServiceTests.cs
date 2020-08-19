using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Laso.Provisioning.Api.V1;
using Laso.Provisioning.FunctionalTests;
using Laso.Provisioning.Infrastructure;
using Laso.TableStorage;
using Microsoft.Extensions.DependencyInjection;
using Provisioning.Domain.Entities;
using Shouldly;
using Xunit;

namespace Laso.Provisioning.IntegrationTests
{
    [Trait("Capability", "Storage Emulator")]   // NOTE: Currently, this is required via configuration.
    [Trait("Capability", "Managed Identity")]   // NOTE: Currently, this is required via configuration.
    public class PartnerServiceTests : FunctionalTestBase
    {

        private readonly Partners.PartnersClient _client;

        public PartnerServiceTests()
        {
            _client = new Partners.PartnersClient(Channel);
        }

        [Fact]
        public async Task When_Resources_Provisioned_Should_Return_Records()
        {
            var storage = Services.GetRequiredService<ITableStorageService>();
            var partnerId = "d36a71c3-343f-4406-a12e-5f30b5d530f8";
            var resourceRecords = new List<ProvisionedResourceEvent>
            {
                new ProvisionedResourceEvent{DisplayName = "Test Cold Storage", Type = ProvisionedResourceType.ColdStorage, Location = "test", ParentLocation = ResourceLocations.GetParentLocationByType(ProvisionedResourceType.ColdStorage), PartnerId = partnerId, ProvisionedOn = DateTime.UtcNow},
                new ProvisionedResourceEvent{DisplayName = "Test Escrow Storage", Type = ProvisionedResourceType.EscrowStorage, Location = "test", ParentLocation = ResourceLocations.GetParentLocationByType(ProvisionedResourceType.EscrowStorage), PartnerId = partnerId, ProvisionedOn = DateTime.UtcNow},
            };

            GetPartnerResourcesReply response;

            try
            {
                await storage.InsertOrReplaceAsync(resourceRecords);
                response = await _client.GetPartnerResourcesAsync(new GetPartnerResourcesRequest{PartnerId = partnerId});
            }
            finally
            {
                await storage.DeleteAsync(resourceRecords);
            }

            response.PartnerId.ShouldMatch(partnerId);
            response.Resources.Count.ShouldBe(2);
        }

        [Fact]
        public async Task When_Partner_History_Requested_Should_Return_All_Records()
        {
            var storage = Services.GetRequiredService<ITableStorageService>();
            var partnerId = "d36a71c3-343f-4406-a12e-5f30b5d530f8";
            var provisioningHistory = new List<ProvisioningActionEvent>
            {
                new ProvisioningActionEvent{Completed = DateTime.UtcNow, Type = ProvisioningActionType.ColdStorageProvisioned, PartnerId = partnerId, ErrorMessage = "Oh my that is not good.  Not at all."},
                new ProvisioningActionEvent{Completed = DateTime.UtcNow, Type = ProvisioningActionType.ColdStorageProvisioned, PartnerId = partnerId},
                new ProvisioningActionEvent{Completed = DateTime.UtcNow, Type = ProvisioningActionType.EscrowStorageProvisioned, PartnerId = partnerId, ErrorMessage = "Oh my that is not good.  Not at all."},
                new ProvisioningActionEvent{Completed = DateTime.UtcNow, Type = ProvisioningActionType.EscrowStorageProvisioned, PartnerId = partnerId},
                new ProvisioningActionEvent{Completed = DateTime.UtcNow, Type = ProvisioningActionType.ColdStorageRemoved, PartnerId = partnerId},
                new ProvisioningActionEvent{Completed = DateTime.UtcNow, Type = ProvisioningActionType.EscrowStorageRemoved, PartnerId = partnerId},
            };

            GetPartnerHistoryReply response;
            try
            {
                await storage.InsertOrReplaceAsync(provisioningHistory);
                response = await _client.GetPartnerHistoryAsync(new GetPartnerHistoryRequest { PartnerId = partnerId });
            }
            finally
            {
                await storage.DeleteAsync(provisioningHistory);
            }

            response.PartnerId.ShouldMatch(partnerId);
            response.Events.Count.ShouldBe(6);
        }
    }
}