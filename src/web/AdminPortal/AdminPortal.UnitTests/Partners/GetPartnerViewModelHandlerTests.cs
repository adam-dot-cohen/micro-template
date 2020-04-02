using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Partners.Queries;
using Laso.AdminPortal.Infrastructure.Partners.Queries;
using Laso.Identity.Api.V1;
using NSubstitute;
using Shouldly;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Laso.AdminPortal.UnitTests.Partners
{
    public class GetPartnerViewModelHandlerTests
    {
        [Fact]
        public async Task When_partner_found_Should_succeed()
        {
            // Arrange
            var partnersClient = Substitute.For<Identity.Api.V1.Partners.PartnersClient>();
            var partner = new PartnerView
            {
                Id = "partnerId",
                Name = "name"
            };
            var getPartnerReply = new GetPartnerReply { Partner = partner };
            partnersClient.GetPartnerAsync(Arg.Any<GetPartnerRequest>())
                .ReturnsForAnyArgs(getPartnerReply.AsGrpcCall());
            var handler = new GetPartnerViewModelHandler(partnersClient);
            var input = new GetPartnerViewModelQuery { PartnerId = partner.Id };

            // Act
            var response = await handler.Handle(input, CancellationToken.None);

            // Assert
            response.ShouldSucceed();
            response.Result.Name.ShouldBe(partner.Name);
            response.Result.Id.ShouldBe(partner.Id);
        }
    }
}
