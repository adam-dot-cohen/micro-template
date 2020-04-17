using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.Provisioning.Core;
using Laso.Provisioning.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Laso.Provisioning.IntegrationTests
{
    [Trait("Capability", "Managed Identity")]   // NOTE: Currently, this is required via configuration.
    [Trait("Capability", "Storage Emulator")]   // NOTE: Currently, this is required via configuration.
    public class SubscriptionProvisioningServiceTests : FunctionalTestBase
    {
        [Fact]
        public async Task When_Invoked_Should_Succeed()
        {
            var provisioningService = Services.GetRequiredService<ISubscriptionProvisioningService>();

            var partnerId = Guid.NewGuid().ToString();
            var partnerName = Faker.Company.Name();

            try
            {
                await provisioningService.ProvisionPartner(partnerId, partnerName, CancellationToken.None);
            }
            catch
            {
                throw;
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

        [Fact]
        public async Task When_Invoked_Twice_Should_Succeed()
        {
            var provisioningService = Services.GetRequiredService<ISubscriptionProvisioningService>();

            var partnerId = Guid.NewGuid().ToString();
            var partnerName = Faker.Company.Name();

            var provisionTask1 =  provisioningService.ProvisionPartner(partnerId, partnerName, CancellationToken.None);
            var provisionTask2 = provisioningService.ProvisionPartner(partnerId, partnerName, CancellationToken.None);
            try
            {
                await provisionTask1;
                await Task.Delay(1000);
                await provisionTask2;
            }
            catch
            {
                throw;
            }
            finally
            {
                await provisioningService.RemovePartner(partnerId, CancellationToken.None);
            }

            // Assert
            provisionTask1.IsCompletedSuccessfully.ShouldBeTrue();
            provisionTask1.Exception.ShouldBeNull();
            provisionTask2.IsCompletedSuccessfully.ShouldBeTrue();
            provisionTask2.Exception.ShouldBeNull();

            var applicationSecrets = Services.GetRequiredService<IApplicationSecrets>();

            var lasoPublicKeyExists = await applicationSecrets.SecretExists($"{partnerId}-laso-pgp-publickey", CancellationToken.None);
            lasoPublicKeyExists.ShouldBeFalse();

            var lasoPrivateKeyExists = await applicationSecrets.SecretExists($"{partnerId}-laso-pgp-privatekey", CancellationToken.None);
            lasoPrivateKeyExists.ShouldBeFalse();
        }
    }
}
