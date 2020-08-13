using Laso.Identity.Api.V1;
using Shouldly;
using Xunit;

namespace Laso.Identity.FunctionalTests.PartnersClient
{
    [Trait("Capability", "Storage Emulator")]   // NOTE: Currently, this is required via configuration.
    public class CreatePartnerTests : FunctionalTestBase
    {
        private readonly Partners.PartnersClient _client;

        public CreatePartnerTests()
        {
            _client = new Partners.PartnersClient(Channel);
        }

        [Fact]
        public void Should_Create_Partner()
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

            var reply = _client.CreatePartner(request);

            reply.ShouldNotBeNull();
            reply.Id.ShouldNotBeNullOrEmpty();
        }
    }
}
