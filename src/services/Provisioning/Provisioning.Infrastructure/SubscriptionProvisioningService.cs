using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laso.IntegrationMessages;
using Laso.Provisioning.Core;
using Laso.Provisioning.Core.Messaging.AzureResources;
using Laso.Provisioning.Core.Messaging.Encryption;
using Laso.Provisioning.Core.Messaging.SFTP;
using Laso.Provisioning.Core.Persistence;
using Laso.Provisioning.Infrastructure.AzureResources;
using Laso.TableStorage;
using Microsoft.Extensions.Logging;
using Provisioning.Domain.Entities;

namespace Laso.Provisioning.Infrastructure
{
    public class SubscriptionProvisioningService : ISubscriptionProvisioningService
    {
        private readonly IApplicationSecrets _applicationSecrets;
        private readonly IDataPipelineStorage _dataPipelineStorage;
        private readonly IEscrowBlobStorageService _escrowBlobStorageService;
        private readonly IColdBlobStorageService _coldBlobStorageService;
        private readonly IMessageSender _bus;
        private readonly ILogger<SubscriptionProvisioningService> _logger;
        private readonly ITableStorageService _tableStorageService;

        private const string EscrowStorage = "EscrowStorage";
        private const string ColdStorage = "ColdStorage";
        private const string ActiveDirectory = "AD";

        private const string SFTPVM = "sFTP";
        
        public SubscriptionProvisioningService(
            IApplicationSecrets applicationSecrets,
            IDataPipelineStorage dataPipelineStorage,
            IMessageSender bus,
            IEscrowBlobStorageService escrowBlobStorageService, 
            IColdBlobStorageService coldBlobStorageService,
            ILogger<SubscriptionProvisioningService> logger, ITableStorageService tableStorageService)
        {
            _applicationSecrets = applicationSecrets;
            _dataPipelineStorage = dataPipelineStorage;
            _bus = bus;
            _escrowBlobStorageService = escrowBlobStorageService;
            _coldBlobStorageService = coldBlobStorageService;
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
            await DeleteDataProcessingDirectories(partnerId, cancellationToken);
            await DeleteColdStorage(partnerId, cancellationToken);
            await DeleteEscrowStorage(partnerId, cancellationToken);
            await DeleteFtpCredentials(partnerId, cancellationToken);
            await DeletePgpKeySetCommand(partnerId, cancellationToken);
            _logger.LogInformation($"Finished initial de-provisioning of partner {partnerId}.");
        }


        private Task DeletePgpKeySetCommand(string partnerId, CancellationToken cancellationToken)
        {
            var pubKeyName = string.Format(ResourceLocations.LASO_PGP_PUBLIC_KEY_FORMAT, partnerId);
            var privKeyName = string.Format(ResourceLocations.LASO_PGP_PRIVATE_KEY_FORMAT, partnerId);
            var passName = string.Format(ResourceLocations.LASO_PGP_PASSPHRASE_FORMAT, partnerId);
            var pEvent = new ProvisioningActionEvent
            {
                PartnerId = partnerId,
                Type = ProvisioningActionType.PGPKeysetRemoved,
                Started = DateTime.UtcNow
            };
            try
            {
                return Task.WhenAll(
                    _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId, $"{ResourceLocations.PARTNERSECRETS}-{pubKeyName}"),
                    _applicationSecrets.DeleteSecret(pubKeyName, cancellationToken),
                    _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId, $"{ResourceLocations.PARTNERSECRETS}-{privKeyName}"),
                    _applicationSecrets.DeleteSecret(privKeyName, cancellationToken),
                    _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId, $"{ResourceLocations.PARTNERSECRETS}-{passName}"),
                    _applicationSecrets.DeleteSecret(passName, cancellationToken))
                    .ContinueWith(t => PersistEvent(pEvent), cancellationToken);

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not remove PGP KeySet for {partnerId}");
                return PersistEvent(pEvent, e);
            }
        }

        
        private Task DeleteFtpCredentials(string partnerId, CancellationToken cancellationToken)
        {
            var pEvent = new ProvisioningActionEvent
            {
                PartnerId = partnerId,
                Type = ProvisioningActionType.FTPCredentialsRemoved,
                Started = DateTime.UtcNow
            };
            var userNLoc = string.Format(ResourceLocations.SFTP_USERNAME_FORMAT, partnerId);
            var pwLoc = string.Format(ResourceLocations.SFTP_PASSWORD_FORMAT, partnerId);
            try
            {
                return Task.WhenAll(
                    _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId, $"{ResourceLocations.PARTNERSECRETS}-{userNLoc}"),
                    _applicationSecrets.DeleteSecret(userNLoc, cancellationToken),
                    _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId, $"{ResourceLocations.PARTNERSECRETS}-{pwLoc}"),
                    _applicationSecrets.DeleteSecret(pwLoc, cancellationToken))
                    .ContinueWith(t => PersistEvent(pEvent),cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not remove FTP Credentials for {partnerId}");
                return PersistEvent(pEvent, e);
            }
        }

        private Task DeleteEscrowStorage(string partnerId, CancellationToken cancellationToken)
        {
            var containerName = StorageResourceNames.GetEscrowContainerName(partnerId);
            var inDirectory = "incoming";
            var outDirectory = "outgoing";
            var pEvent = new ProvisioningActionEvent
            {
                PartnerId = partnerId,
                Type = ProvisioningActionType.EscrowStorageRemoved,
                Started = DateTime.UtcNow
            };
            try
            {
                return Task.WhenAll(
                        _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId,
                            $"{EscrowStorage}-{containerName}"),
                        _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId,
                            $"{EscrowStorage}-{containerName}-{inDirectory}"),
                        _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId,
                            $"{EscrowStorage}-{containerName}-{outDirectory}"),
                        _escrowBlobStorageService.DeleteContainerIfExists(containerName, cancellationToken)
                        ).ContinueWith(t => PersistEvent(pEvent),cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not remove Escrow Storage for {partnerId}");
                return PersistEvent(pEvent, e);
            }
        }

        private Task DeleteColdStorage(string partnerId, CancellationToken cancellationToken)
        {
            var pEvent = new ProvisioningActionEvent
            {
                PartnerId = partnerId,
                Type = ProvisioningActionType.ColdStorageRemoved,
                Started = DateTime.UtcNow
            };
            try
            {
                return Task.WhenAll(_coldBlobStorageService.DeleteContainerIfExists(partnerId, cancellationToken),
                    _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId,$"{ColdStorage}-{partnerId}"))
                    .ContinueWith(t => PersistEvent(pEvent),cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not remove Cold Storage for {partnerId}");
                return PersistEvent(pEvent, e);
            }
        }

        private Task DeleteFTPAccount(string partnerId, CancellationToken cancellationToken)
        {
            var pEvent = new ProvisioningActionEvent
            {
                PartnerId = partnerId,
                Started = DateTime.UtcNow,
                Type = ProvisioningActionType.FTPAccountRemoved
            };
            var userNameSecretExistsTask =
                _applicationSecrets.SecretExists(string.Format(ResourceLocations.SFTP_USERNAME_FORMAT, partnerId),
                    cancellationToken);
            try
            {
                return userNameSecretExistsTask.ContinueWith(ut =>
                {
                    if (!userNameSecretExistsTask.Result)
                    {
                        PersistEvent(pEvent, new ArgumentOutOfRangeException(
                            $"No user name found for {partnerId}.  You may have to issue the command manually."));
                    }
                    else
                    {
                        var getUserNameTask = _applicationSecrets.GetSecret(string.Format(ResourceLocations.SFTP_USERNAME_FORMAT, partnerId), cancellationToken);
                        getUserNameTask.Wait(cancellationToken);
                        var cmd = new DeletePartnerAccountCommand
                        {
                            PartnerId = Guid.Parse(partnerId),
                            AccountName = getUserNameTask.Result
                        };
                        Task.WhenAll(
                                _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId,
                                    $"{SFTPVM}-{getUserNameTask.Result}"),
                                _bus.Send(cmd))
                            .ContinueWith(t => PersistEvent(pEvent), cancellationToken);
                    }
                },cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not remove FTP Account for {partnerId}");
                return PersistEvent(pEvent, e);
            }
        }


        private Task DeleteDataProcessingDirectories(string partnerId, CancellationToken cancellationToken)
        {
            var pEvent = new ProvisioningActionEvent
            {
                PartnerId = partnerId,
                Type = ProvisioningActionType.DataProcessingDirectoriesRemoved,
                Started = DateTime.UtcNow
            };
            try
            {
                var deleteDirectoryTasks = StorageResourceNames.GetDataResourceNames()
                    .Select(f => Task.WhenAll( 
                        _dataPipelineStorage.CreateDirectory(f.Key, partnerId, cancellationToken),
                        _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId, $"{f.Key}-{partnerId}")));
                return Task.WhenAll(deleteDirectoryTasks).ContinueWith(t => PersistEvent(pEvent), cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not remove Data Processing Directories for {partnerId}");
                return PersistEvent(pEvent, e);
            }
        }

        private Task PersistEvent(ProvisioningActionEvent pEvent, Exception e = null)
        {
            pEvent.Completed = DateTime.UtcNow;
            pEvent.ErrorMessage = e?.Message ?? string.Empty;
            return _tableStorageService.InsertOrReplaceAsync(pEvent);
        }
    }
}
