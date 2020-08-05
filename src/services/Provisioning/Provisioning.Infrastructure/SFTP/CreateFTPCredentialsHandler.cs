using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.IntegrationEvents;
using Laso.IntegrationMessages;
using Laso.Provisioning.Core;
using Laso.Provisioning.Core.Messaging.SFTP;
using Laso.Provisioning.Infrastructure.Utilities;
using Laso.TableStorage;
using Microsoft.Extensions.Logging;
using Provisioning.Domain.Entities;

namespace Laso.Provisioning.Infrastructure.SFTP
{
    public class CreateFTPCredentialsHandler : ICommandHandler<CreateFTPCredentialsCommand>
    {
        private IEventPublisher _eventPublisher;
        private IApplicationSecrets _secrets;
        private ITableStorageService _tableStorage;
        private ILogger<CreateFTPCredentialsHandler> _logger;


        public CreateFTPCredentialsHandler(IEventPublisher eventPublisher, IApplicationSecrets secrets, ITableStorageService tableStorage, ILogger<CreateFTPCredentialsHandler> logger)
        {
            _eventPublisher = eventPublisher;
            _secrets = secrets;
            _tableStorage = tableStorage;
            _logger = logger;
        }

        public Task Handle(CreateFTPCredentialsCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Creating FTP Credentials for {command.PartnerName}, Id: {command.PartnerId}");

            var userNLoc = string.Format(ResourceLocations.SFTP_USERNAME_FORMAT, command.PartnerId);
            var pwLoc = string.Format(ResourceLocations.SFTP_PASSWORD_FORMAT, command.PartnerId);
            var checkStateTask = _tableStorage.GetAsync<ProvisionedResourceEvent>(command.PartnerId, $"{ResourceLocations.PARTNERSECRETS}-{userNLoc}");
            checkStateTask.Wait(cancellationToken);
            var checkpwState = _tableStorage.GetAsync<ProvisionedResourceEvent>(command.PartnerId,
                $"{ResourceLocations.PARTNERSECRETS}-{pwLoc}");
            checkpwState.Wait(cancellationToken);
            if (checkStateTask.Result != null && checkpwState.Result != null)
                return _eventPublisher.Publish(new FTPCredentialsCreatedEvent {PartnerId = command.PartnerId});

            var userName = $"{command.PartnerName}{StringUtilities.GetRandomString(4, "0123456789")}";
            var password = StringUtilities.GetRandomAlphanumericString(10);
            try
            {
                if (checkStateTask.Result == null)
                {
                    var uName = _secrets.SetSecret(userNLoc, userName, cancellationToken);
                    uName.Wait(cancellationToken);
                    if (string.IsNullOrWhiteSpace(uName.Result))
                        return _eventPublisher.Publish(new FTPCredentialsCreationFailedEvent
                            {PartnerId = command.PartnerId, Reason = "Unable to set Username Secret"});

                    var uNameRec = _tableStorage.InsertAsync(new ProvisionedResourceEvent
                    {
                        PartnerId = command.PartnerId,
                        Type = ProvisionedResourceType.SFTPUsername,
                        ParentLocation =
                            ResourceLocations.GetParentLocationByType(ProvisionedResourceType.SFTPUsername),
                        Location = userNLoc,
                        DisplayName = "sFTP User Name",
                        ProvisionedOn = DateTime.UtcNow
                    });
                    uNameRec.Wait(cancellationToken);
                }

                if (checkpwState.Result == null)
                {
                    var pwSecret = _secrets.SetSecret(pwLoc, password, cancellationToken);
                    pwSecret.Wait(cancellationToken);
                    if (string.IsNullOrWhiteSpace(pwSecret.Result))
                        return _eventPublisher.Publish(new FTPCredentialsCreationFailedEvent { PartnerId = command.PartnerId, Reason = "Unable to create FTP Password secret."});
                    var pwRec = _tableStorage.InsertAsync(new ProvisionedResourceEvent
                    {
                        PartnerId = command.PartnerId,
                        Type = ProvisionedResourceType.SFTPPassword,
                        ParentLocation =
                            ResourceLocations.GetParentLocationByType(ProvisionedResourceType.SFTPPassword),
                        Location = pwLoc,
                        DisplayName = "sFTP Password",
                        Sensitive = true,
                        ProvisionedOn = DateTime.UtcNow
                    });
                    pwRec.Wait(cancellationToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred while creating credentials for {command.PartnerId}.  Command can be re-sent.");
                return _eventPublisher.Publish(new FTPCredentialsCreationFailedEvent
                    {PartnerId = command.PartnerId, Reason = e.Message});
            }

            return _eventPublisher.Publish(new FTPCredentialsCreatedEvent {PartnerId = command.PartnerId, UsernameSecret = userNLoc, PasswordSecret = pwLoc});
        }
    }
}