using System.Threading.Tasks;
using Laso.Identity.Api.V1;
using Laso.Testing;
using Shouldly;
using Xunit;

namespace Laso.Identity.FunctionalTests.PartnersClient
{
    [Trait("Capability", "Storage")]   // NOTE: Currently, this is required via configuration.
    public class CreatePartnerTests : FunctionalTestBase<Laso.Identity.Api.Program>
    {
        private readonly Partners.PartnersClient _client;

        public CreatePartnerTests()
        {
            _client = new Partners.PartnersClient(Channel);
        }

        [Fact]
        public async Task Should_Create_Partner()
        {
            // Arrange, Act
            var reply = await CreateFakePartner();

            // Assert
            reply.ShouldNotBeNull();
            reply.Id.ShouldNotBeNullOrEmpty();
        }

        private Task<CreatePartnerReply> CreateFakePartner()
        {
            return CreateFakePartner(_client);
        }

        public static Task<CreatePartnerReply> CreateFakePartner(Partners.PartnersClient client)
        {
            var request = new CreatePartnerRequest
            {
                Partner = new PartnerModel
                {
                    Name = Faker.Company.Name(),
                    ContactName = Faker.Name.FullName(),
                    ContactPhone = Faker.Phone.Number(),
                    ContactEmail = Faker.Internet.Email()
                }
            };

            return client.CreatePartnerAsync(request).ResponseAsync;
        }
    }
}
