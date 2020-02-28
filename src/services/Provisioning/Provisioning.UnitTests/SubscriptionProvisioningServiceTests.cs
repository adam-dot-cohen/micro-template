using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.Provisioning.Core;
using Laso.Provisioning.Infrastructure;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Laso.Provisioning.UnitTests
{
    public class SubscriptionProvisioningServiceTests
    {
        [Fact]
        public async Task When_Invoked_Should_Succeed()
        {
            // Arrange
            var keyVaultService = new InMemoryKeyVaultService();
            var eventPublisher = Substitute.For<IEventPublisher>();

            var provisioningService = new SubscriptionProvisioningService(keyVaultService, eventPublisher);
            var partnerId = Guid.NewGuid();

            // Act
            await provisioningService.ProvisionPartner(partnerId.ToString(), "somepartner", CancellationToken.None);

            // Assert
            keyVaultService.Secrets.Count.ShouldBe(5);
            keyVaultService.Secrets[$"{partnerId}-partner-ftp-username"].ShouldNotBeNullOrEmpty();
            keyVaultService.Secrets[$"{partnerId}-partner-ftp-username"].Length.ShouldBeGreaterThan(5);
            keyVaultService.Secrets[$"{partnerId}-partner-ftp-password"].ShouldNotBeNullOrEmpty();
            keyVaultService.Secrets[$"{partnerId}-partner-ftp-password"].Length.ShouldBe(10);
            //keyVaultService.Secrets[$"{partnerId}-laso-pgp-publickey"].ShouldNotBeNullOrEmpty();
            //keyVaultService.Secrets[$"{partnerId}-laso-pgp-privatekey"].ShouldNotBeNullOrEmpty();
            //keyVaultService.Secrets[$"{partnerId}-laso-pgp-passphrase"].ShouldNotBeNullOrEmpty();
        }
    }
}
