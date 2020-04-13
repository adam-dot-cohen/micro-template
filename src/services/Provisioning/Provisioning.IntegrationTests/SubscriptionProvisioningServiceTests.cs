using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.Provisioning.Core;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Laso.Provisioning.IntegrationTests
{
    public class SubscriptionProvisioningServiceTests : IntegrationTestBase
    {
        [Fact(Skip = "Disable until Access Token acquired.")]
        public async Task When_Invoked_Should_Succeed()
        {
            var provisioningService = Services.GetRequiredService<ISubscriptionProvisioningService>();

            var partnerId = Guid.NewGuid().ToString();
            var partnerName = Faker.Company.Name();

            try
            {
                await provisioningService.ProvisionPartner(partnerId, partnerName, CancellationToken.None);
            }
            finally
            {
                await provisioningService.RemovePartner(partnerId, CancellationToken.None);
            }

            // Assert
            var applicationSecrets = Services.GetRequiredService<IApplicationSecrets>();

            var lasoPublicKeyExists = await applicationSecrets.SecretExists($"{partnerId}-laso-pgp-publickey", CancellationToken.None);
            lasoPublicKeyExists.ShouldBeFalse();

            var lasoPrivateKeyExists = await applicationSecrets.SecretExists($"{partnerId}-laso-pgp-privatekey", CancellationToken.None);
            lasoPrivateKeyExists.ShouldBeFalse();
        }
    }
}
