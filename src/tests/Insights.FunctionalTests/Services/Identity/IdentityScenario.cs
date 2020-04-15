using System;
using System.Threading.Tasks;
using Laso.Identity.Api.V1;
using Laso.Identity.Core.Persistence;
using Laso.Identity.Domain.Entities;
using Laso.Identity.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Laso.Insights.FunctionalTests.Services.Identity
{
    public class IdentityScenario : FunctionalTestBase
    {
        public IdentityScenario()
        {
            Host.ShouldNotBeNull();
        }
        
        public IdentityScenarioContext Context { get; } = new IdentityScenarioContext();

        public async Task<IdentityScenario> CreatePartner()
        {
            var client = new Partners.PartnersClient(Channel);

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

            var reply = await client.CreatePartnerAsync(request);

            reply.ShouldNotBeNull();
            reply.Id.ShouldNotBeNullOrEmpty();

            Context.PartnerId = reply.Id;

            return this;
        }

        // REVIEW: This should probably be turned into a command, or something that better
        // encapsulates the business logic of deleting a partner. [jay_mclain]
        public async Task<IdentityScenario> DeletePartner()
        {
            if (Context.PartnerId == null)
                throw new InvalidOperationException("Partner has not been created.");

            var tableStorageService = Services.GetRequiredService<ITableStorageService>();
            await tableStorageService.DeleteAsync<Partner>(Context.PartnerId);

            Context.PartnerId = null;

            return this;
        }

        public class IdentityScenarioContext
        {
            public string PartnerId { get; set; }
        }
    }
}
