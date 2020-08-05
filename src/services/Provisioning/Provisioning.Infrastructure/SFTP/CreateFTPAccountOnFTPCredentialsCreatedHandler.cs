using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.IntegrationEvents;
using Laso.IntegrationMessages;
using Laso.Provisioning.Core;
using Laso.Provisioning.Core.Messaging.SFTP;
using Laso.Provisioning.Infrastructure.AzureResources;
using Microsoft.Extensions.Logging;

namespace Laso.Provisioning.Infrastructure.SFTP
{
    public class CreateFTPAccountOnFTPCredentialsCreatedHandler : IEventHandler<FTPCredentialsCreatedEvent>
    {
        private IMessageSender _bus;
        private ILogger<CreateFTPAccountOnFTPCredentialsCreatedHandler> _logger;
        private IApplicationSecrets _secrets;

        public CreateFTPAccountOnFTPCredentialsCreatedHandler(IMessageSender bus, IApplicationSecrets secrets, ILogger<CreateFTPAccountOnFTPCredentialsCreatedHandler> logger)
        {
            _bus = bus;
            _secrets = secrets;
            _logger = logger;
        }

        public Task Handle(FTPCredentialsCreatedEvent @event)
        {
            if (string.IsNullOrWhiteSpace(@event.PasswordSecret) || string.IsNullOrWhiteSpace(@event.UsernameSecret))
            {
                _logger.LogError($"The event for {@event.PartnerId} was missing at least one of the secret locations.  Re-issue event with secret locations or manually submit the CreatePartnerAccountCommand.");
                return Task.CompletedTask;
            }

            if (!SecretsExist(@event.UsernameSecret, @event.PasswordSecret))
            {
                _logger.LogError($"At least one of the secrets for {@event.PartnerId} was not found in the Key Vault. Re-issue event with secret locations or manually submit the CreatePartnerAccountCommand.");
                return Task.CompletedTask;
            }

            var getUserName = _secrets.GetSecret(@event.UsernameSecret, CancellationToken.None);
            getUserName.Wait();
            var getPassword = _secrets.GetSecret(@event.PasswordSecret, CancellationToken.None);
            getPassword.Wait();
            return _bus.Send(new CreatePartnerAccountCommand
            {
                AccountName = getUserName.Result, 
                PartnerId = Guid.Parse(@event.PartnerId), 
                Password = getPassword.Result, 
                Container = StorageResourceNames.GetEscrowContainerName(@event.PartnerId)
            });
        }

        public bool SecretsExist(string userNameSecret, string passwordSecret)
        {
            var uNameExists = _secrets.SecretExists(userNameSecret, CancellationToken.None);
            uNameExists.Wait();
            if (!uNameExists.Result)
                return false;

            var pwExists = _secrets.SecretExists(passwordSecret, CancellationToken.None);
            pwExists.Wait();
            return pwExists.Result;
        }
    }
}