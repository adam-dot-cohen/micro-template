using System.Threading;
using Laso.AdminPortal.Infrastructure.Partners.Queries;
using Laso.Identity.Api.V1;
using Laso.Provisioning.Api.V1;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Laso.AdminPortal.UnitTests.Partners
{
    public class GetPartnerProvisioningHistoryViewModelHandlerTests
    {
        [Fact]
        public void When_Partner_Not_Found_Should_Return_Failed()
        {
            var identityService = Substitute.For<Identity.Api.V1.Partners.PartnersClient>();
            var provisioningService = Substitute.For<Provisioning.Api.V1.Partners.PartnersClient>();

            var noPartnerReply = new GetPartnerReply();
            identityService.GetPartnerAsync(Arg.Any<GetPartnerRequest>())
                .Returns(noPartnerReply.AsGrpcCall());

            var sut = new GetPartnerProvisioningHistoryViewModelHandler(identityService, provisioningService);
            var result =
                sut.Handle(new Laso.AdminPortal.Core.Partners.Queries.GetPartnerProvisioningHistoryViewModelQuery {Id = "12343"}, CancellationToken.None).Result;
            result.Success.ShouldBeFalse();
            result.ValidationMessages.ShouldContain(vm => vm.Key.Contains("Partner"));
        }

        [Fact]
        public void When_History_Not_Found_Should_Return_Failed()
        {
            var partnerId = "1234";
            var identityService = Substitute.For<Identity.Api.V1.Partners.PartnersClient>();
            var provisioningService = Substitute.For<Provisioning.Api.V1.Partners.PartnersClient>();
            var partnerIdentityReply = new GetPartnerReply
                {Partner = new PartnerView {Id = partnerId, Name = "Test Partner"}};
            identityService.GetPartnerAsync(Arg.Any<GetPartnerRequest>())
                .Returns(partnerIdentityReply.AsGrpcCall());
            var noHistoryFoundReply = new GetPartnerHistoryReply();
            provisioningService.GetPartnerHistoryAsync(Arg.Any<GetPartnerHistoryRequest>())
                .Returns(noHistoryFoundReply.AsGrpcCall());
            var sut = new GetPartnerProvisioningHistoryViewModelHandler(identityService, provisioningService);
            var result =
                sut.Handle(new Laso.AdminPortal.Core.Partners.Queries.GetPartnerProvisioningHistoryViewModelQuery { Id = partnerId }, CancellationToken.None).Result;
            result.Success.ShouldBeFalse();
            result.ValidationMessages.ShouldContain(vm => vm.Key.Contains("ProvisioningHistory"));
        }

        [Fact]
        public void When_History_Is_Empty_Should_Return_Success()
        {
            var partnerId = "1234";
            var identityService = Substitute.For<Identity.Api.V1.Partners.PartnersClient>();
            var provisioningService = Substitute.For<Provisioning.Api.V1.Partners.PartnersClient>();
            var partnerIdentityReply = new GetPartnerReply
                {Partner = new PartnerView {Id = partnerId, Name = "Test Partner"}};
            identityService.GetPartnerAsync(Arg.Any<GetPartnerRequest>())
                .Returns(partnerIdentityReply.AsGrpcCall());
            var noHistoryReply = new GetPartnerHistoryReply{PartnerId = partnerId};
            provisioningService.GetPartnerHistoryAsync(Arg.Any<GetPartnerHistoryRequest>())
                .Returns(noHistoryReply.AsGrpcCall());
            var sut = new GetPartnerProvisioningHistoryViewModelHandler(identityService, provisioningService);
            var result =
                sut.Handle(new Laso.AdminPortal.Core.Partners.Queries.GetPartnerProvisioningHistoryViewModelQuery { Id = partnerId }, CancellationToken.None).Result;
            result.Success.ShouldBeTrue();
            result.Result.Name.ShouldBe(partnerIdentityReply.Partner.Name);
        }

        [Fact]
        public void When_History_Returned_Should_Succeed()
        {
            var partnerId = "1234";
            var identityService = Substitute.For<Identity.Api.V1.Partners.PartnersClient>();
            var provisioningService = Substitute.For<Provisioning.Api.V1.Partners.PartnersClient>();
            var partnerIdentityReply = new GetPartnerReply
                {Partner = new PartnerView {Id = partnerId, Name = "Test Partner"}};
            identityService.GetPartnerAsync(Arg.Any<GetPartnerRequest>())
                .Returns(partnerIdentityReply.AsGrpcCall());
            var historyFoundReply = new GetPartnerHistoryReply{PartnerId = partnerId};
            historyFoundReply.Events.Add(new ProvisioningEventView{Sequence = 1});
            historyFoundReply.Events.Add(new ProvisioningEventView{Sequence = 2});
            historyFoundReply.Events.Add(new ProvisioningEventView{Sequence = 3});
            historyFoundReply.Events.Add(new ProvisioningEventView{Sequence = 4});
            provisioningService.GetPartnerHistoryAsync(Arg.Any<GetPartnerHistoryRequest>())
                .Returns(historyFoundReply.AsGrpcCall());
            var sut = new GetPartnerProvisioningHistoryViewModelHandler(identityService, provisioningService);
            var result =
                sut.Handle(new Laso.AdminPortal.Core.Partners.Queries.GetPartnerProvisioningHistoryViewModelQuery { Id = partnerId }, CancellationToken.None).Result;
            result.Success.ShouldBeTrue();
            result.Result.History.Count.ShouldBe(4);
        }
    }
}
