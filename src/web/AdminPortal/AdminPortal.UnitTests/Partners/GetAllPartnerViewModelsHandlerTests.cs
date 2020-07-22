using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Partners.Queries;
using Laso.AdminPortal.Infrastructure.Partners.Queries;
using Laso.AdminPortal.UnitTests.Extensions;
using Laso.Identity.Api.V1;
using NSubstitute;
using Shouldly;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Laso.AdminPortal.UnitTests.Partners
{
    public class GetAllPartnerViewModelsHandlerTests
    {
        [Fact]
        public async Task When_getting_all_partners_Should_succeed()
        {
            // Arrange
            var partnersClient = Substitute.For<Identity.Api.V1.Partners.PartnersClient>();
            var partners = new[]
            {
                new PartnerView
                {
                    Id = "1",
                    Name = "name1"
                },
                new PartnerView
                {
                    Id = "2",
                    Name = "a name2"
                }
            };
            var reply = new GetPartnersReply();
            reply.Partners.AddRange(partners);

            partnersClient.GetPartnersAsync(Arg.Any<GetPartnersRequest>())
                .ReturnsForAnyArgs(reply.AsGrpcCall());
            var handler = new GetAllPartnerViewModelsHandler(partnersClient);
            var input = new GetAllPartnerViewModelsQuery();

            // Act
            var response = await handler.Handle(input, CancellationToken.None);

            // Assert
            response.ShouldSucceed();
            response.Result.Count.ShouldBe(2);
            response.Result.Any(p => p.Id == "1").ShouldBeTrue();
            response.Result.Any(p => p.Id == "2").ShouldBeTrue();
            response.Result.First().Id.ShouldBe("2"); // order by name
        }
    }
}
