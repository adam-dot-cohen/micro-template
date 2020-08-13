using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.IntegrationMessages;
using Laso.Provisioning.Core;
using Laso.Provisioning.Core.Messaging.AzureResources;
using Laso.Provisioning.Core.Messaging.Encryption;
using Laso.Provisioning.Core.Messaging.SFTP;
using Laso.Provisioning.Core.Persistence;
using Laso.TableStorage;
using Microsoft.Extensions.Logging;
using Provisioning.Domain.Entities;

namespace Laso.Provisioning.Infrastructure
{
    public class SubscriptionProvisioningService : ISubscriptionProvisioningService
    {
        private readonly IApplicationSecrets _applicationSecrets;
        private readonly IMessageSender _bus;
        private readonly ILogger<SubscriptionProvisioningService> _logger;
        private readonly ITableStorageService _tableStorageService;

        private const string SFTPVM = "sFTP";
        
        public SubscriptionProvisioningService(
            IApplicationSecrets applicationSecrets,
            IMessageSender bus,
            ILogger<SubscriptionProvisioningService> logger, 
            ITableStorageService tableStorageService)
        {
            _applicationSecrets = applicationSecrets;
            _bus = bus;
            _logger = logger;
            _tableStorageService = tableStorageService;
        }

        // TODO: Consider compensating commands for failure conditions.
        public async Task ProvisionPartner(string partnerId, string partnerName, CancellationToken cancellationToken)
        {
            // Create public/private PGP Encryption key pair.
            await _bus.Send(new CreatePgpKeySetCommand{PartnerId = partnerId});

            // Create FTP account credentials (this could be queued work, if we have a process manager)
            await _bus.Send(new CreateFTPCredentialsCommand{PartnerId = partnerId, PartnerName = partnerName});

            // Create Blob Container with incoming and outgoing directories
            await _bus.Send(new CreatePartnerEscrowStorageCommand{ PartnerId = partnerId, PartnerName = partnerName });

            await _bus.Send(new CreatePartnerColdStorageCommand{PartnerName = partnerName, PartnerId = partnerId});

            // TODO: Configure experiment storage? Or wait for data?

            await _bus.Send(
                new CreatePartnerDataProcessingDirCommand {PartnerId = partnerId, PartnerName = partnerName});

            // TODO: Ensure/Create partner AD Group
            // TODO: Assign Permissions to provisioned resources (e.g. DataProcessingDirectories)

            _logger.LogInformation($"Finished initial provisioning of partner {partnerId}.");
        }

        public async Task RemovePartner(string partnerId, CancellationToken cancellationToken)
        {
            await DeleteFTPAccount(partnerId, cancellationToken);
            await _bus.Send(new DeletePartnerDataProcessingDirCommand{PartnerId = partnerId});
            await _bus.Send(new DeletePartnerColdStorageCommand{PartnerId = partnerId});
            await _bus.Send(new DeletePartnerEscrowStorageCommand{PartnerId = partnerId});
            await _bus.Send(new DeletePartnerFTPCredentialsCommand {PartnerId = partnerId});
            await _bus.Send(new DeletePartnerPgpKeysCommand{PartnerId = partnerId});
            _logger.LogInformation($"Finished initial de-provisioning of partner {partnerId}.");
        }


        private Task DeleteFTPAccount(string partnerId, CancellationToken cancellationToken)
        {
            var userNameSecretExistsTask =
                _applicationSecrets.SecretExists(string.Format(ResourceLocations.SFTP_USERNAME_FORMAT, partnerId),
                    cancellationToken);
            userNameSecretExistsTask.Wait(cancellationToken);
            if (!userNameSecretExistsTask.Result)
            {
                _logger.LogError($"No user name found for {partnerId}.  You may have to issue the command manually.");
                return Task.CompletedTask;
            }

            var getUserNameTask = _applicationSecrets.GetSecret(string.Format(ResourceLocations.SFTP_USERNAME_FORMAT, partnerId), cancellationToken);
            getUserNameTask.Wait(cancellationToken);
            var cmd = new DeletePartnerAccountCommand
            {
                PartnerId = Guid.Parse(partnerId),
                AccountName = getUserNameTask.Result
            };
            return Task.WhenAll(
                _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId,
                    $"{SFTPVM}-{getUserNameTask.Result}"),
                _bus.Send(cmd));
        }
    }
}
