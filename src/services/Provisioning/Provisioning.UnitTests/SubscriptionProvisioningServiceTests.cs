using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Laso.IntegrationMessages;
using Laso.Provisioning.Core.Messaging.AzureResources;
using Laso.Provisioning.Core.Messaging.Encryption;
using Laso.Provisioning.Core.Messaging.SFTP;
using Laso.Provisioning.Infrastructure;
using Laso.TableStorage;
using Microsoft.Extensions.Logging.Abstractions;
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
            var keyVaultService = new InMemoryApplicationSecrets();
            var messageSender = new TestMessageSender();
            var logger = new NullLogger<SubscriptionProvisioningService>();
            var provisioningStorage = Substitute.For<ITableStorageService>();

            var provisioningService = new SubscriptionProvisioningService(keyVaultService, messageSender, logger, provisioningStorage);
            var partnerId = Guid.NewGuid();

            // Act
            await provisioningService.ProvisionPartner(partnerId.ToString(), "somepartner", CancellationToken.None);

            // Assert
            messageSender.Received<CreatePgpKeySetCommand>().ShouldBeTrue();
            messageSender.Received<CreateFTPCredentialsCommand>().ShouldBeTrue();
            messageSender.Received<CreatePartnerEscrowStorageCommand>().ShouldBeTrue();
            messageSender.Received<CreatePartnerColdStorageCommand>().ShouldBeTrue();
            messageSender.Received<CreatePartnerDataProcessingDirCommand>().ShouldBeTrue();
            messageSender.Received<CreatePartnerAccountCommand>().ShouldBeFalse();
        }
    }

    public class TestMessageSender : IMessageSender 
    {
        private readonly List<IIntegrationMessage> _recievedMessages = new List<IIntegrationMessage>();

        public Task Send<T>(T message) where T : IIntegrationMessage
        {
            _recievedMessages.Add(message);
            return Task.CompletedTask;
        }

        public bool Received<T>() where T : IIntegrationMessage
        {
            return _recievedMessages.Exists(message => message.GetType() == typeof(T));
        }
    }
}
