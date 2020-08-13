using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.IntegrationEvents;
using Laso.IntegrationMessages;
using Laso.Provisioning.Core;
using Laso.Provisioning.Core.Messaging.SFTP;
using Laso.TableStorage;
using Microsoft.Extensions.Logging;
using Provisioning.Domain.Entities;

namespace Laso.Provisioning.Infrastructure.SFTP
{
    public class DeletePartnerFTPCredentialsHandler : ICommandHandler<DeletePartnerFTPCredentialsCommand>
    {

        private IEventPublisher _eventPublisher;
        private IApplicationSecrets _secrets;
        private ITableStorageService _tableStorage;
        private ILogger<CreateFTPCredentialsHandler> _logger;

        public DeletePartnerFTPCredentialsHandler(IEventPublisher eventPublisher, IApplicationSecrets secrets, ITableStorageService tableStorage, ILogger<CreateFTPCredentialsHandler> logger)
        {
            _eventPublisher = eventPublisher;
            _secrets = secrets;
            _tableStorage = tableStorage;
            _logger = logger;
        }

        public Task Handle(DeletePartnerFTPCredentialsCommand command, CancellationToken cancellationToken)
        {

            _logger.LogInformation($"removing FTP Credentials for partner: {command.PartnerId}");

            var userNLoc = string.Format(ResourceLocations.SFTP_USERNAME_FORMAT, command.PartnerId);
            var pwLoc = string.Format(ResourceLocations.SFTP_PASSWORD_FORMAT, command.PartnerId);

            try
            {
                var removeUN = _secrets.DeleteSecret(userNLoc, cancellationToken);
                removeUN.Wait(cancellationToken);
                var removeRecord = _tableStorage.DeleteAsync<ProvisionedResourceEvent>(command.PartnerId, $"{ResourceLocations.GetParentLocationByType(ProvisionedResourceType.SFTPUsername)}-{userNLoc}");
                removeRecord.Wait(cancellationToken);
                var removePW = _secrets.DeleteSecret(pwLoc, cancellationToken);
                removePW.Wait(cancellationToken);
                var removePWRecord =_tableStorage.DeleteAsync<ProvisionedResourceEvent>(command.PartnerId, $"{ResourceLocations.GetParentLocationByType(ProvisionedResourceType.SFTPPassword)}-{pwLoc}");
                removePWRecord.Wait(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred while removing credentials for {command.PartnerId}.  Command can be re-sent.");
                throw;
            }

            return _eventPublisher.Publish(new PartnerFTPCredentialsDeletedEvent{Completed = DateTime.UtcNow, PartnerId = command.PartnerId});
        }
    }
}