using System.Threading.Tasks;
using Laso.Identity.Api.V1;
using Shouldly;
using Xunit;

namespace Laso.Identity.FunctionalTests.PartnersClient
{
    public class DeletePartnerTests : FunctionalTestBase
    {
        private readonly Partners.PartnersClient _client;

        public DeletePartnerTests()
        {
            _client = new Partners.PartnersClient(Channel);
        }

        [Fact]
        public async Task Should_Delete_Partner()
        {
            // Arrange
            var createPartnerReply = await CreatePartnerTests.CreateFakePartner(_client);
            createPartnerReply.ShouldNotBeNull();

            // Act
            var reply = await DeletePartner(_client, createPartnerReply.Id);

            // Assert
            reply.ShouldNotBeNull();
        }

        public static Task<DeletePartnerReply> DeletePartner(Partners.PartnersClient client, string id)
        {
            var request = new DeletePartnerRequest { Id = id };

            return client.DeletePartnerAsync(request).ResponseAsync;
        }
    }
}
