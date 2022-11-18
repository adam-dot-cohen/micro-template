using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Testing;
using Grpc.Core.Utils;
using Laso.Provisioning.Api.Services;
using Laso.Provisioning.Api.V1;
using Laso.Provisioning.Core;
using Laso.TableStorage;
using NSubstitute;
using Provisioning.Domain.Entities;
using Shouldly;
using Xunit;

namespace Laso.Provisioning.UnitTests
{
    public class PartnerServiceTests
    {
        [Fact]
        public async void When_Resources_Exists_Should_Return_Them()
        {
            var partnerId = Guid.NewGuid().ToString();
            var resources = new List<ProvisionedResourceEvent>
            {
                new ProvisionedResourceEvent{DisplayName = "Test",PartnerId = partnerId, Sensitive = false, ProvisionedOn = DateTime.UtcNow, Type = ProvisionedResourceType.ColdStorage, Location = "Test"}
            };
            var tableStorage = Substitute.For<ITableStorageService>();
            tableStorage.GetAllAsync<ProvisionedResourceEvent>()
                .Returns(Task.FromResult<ICollection<ProvisionedResourceEvent>>(resources));
            var resourceLocator = Substitute.For<IResourceLocator>();
            resourceLocator.GetLocationString(Arg.Any<ProvisionedResourceEvent>()).Returns("Test Value");
            var context = TestServerCallContext.Create("GetPartnerResources", null, DateTime.UtcNow.AddHours(1),
                new Metadata(), CancellationToken.None, "127.0.0.1", null, null, (metadata) => TaskUtils.CompletedTask,
                () => new WriteOptions(), (writeOptions) => { });
            var sut = new PartnersServiceV1(tableStorage, Substitute.For<ISubscriptionProvisioningService>(),
                resourceLocator);
            var reply = await sut.GetPartnerResources(new GetPartnerResourcesRequest{PartnerId = partnerId}, context);
            reply.PartnerId.ShouldBe(partnerId);
            reply.Resources.Count.ShouldBe(1);
        }
    }
}