using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laso.IntegrationEvents;
using Laso.IntegrationMessages;
using Laso.Provisioning.Core;
using Laso.Provisioning.Core.Messaging.Encryption;
using Laso.TableStorage;
using Microsoft.Extensions.Logging;
using Provisioning.Domain.Entities;

namespace Laso.Provisioning.Infrastructure.Encryption
{
    public class DeletePartnerPgpKeysHandler : ICommandHandler<DeletePartnerPgpKeysCommand>
    {

        private readonly ILogger<CreatePgpKeySetHandler> _logger;
        private readonly IEventPublisher _bus;
        private readonly IApplicationSecrets _secrets;
        private readonly ITableStorageService _tableStorage;

        public DeletePartnerPgpKeysHandler(ILogger<CreatePgpKeySetHandler> logger, IEventPublisher bus, IApplicationSecrets secrets, ITableStorageService tableStorage)
        {
            _logger = logger;
            _bus = bus;
            _secrets = secrets;
            _tableStorage = tableStorage;
        }

        public Task Handle(DeletePartnerPgpKeysCommand command, CancellationToken cancellationToken)
        {

            _logger.LogInformation($"Removing partner {command.PartnerId} PGP key set.");

            var pubKeyName = string.Format(ResourceLocations.LASO_PGP_PUBLIC_KEY_FORMAT, command.PartnerId);
            var privKeyName = string.Format(ResourceLocations.LASO_PGP_PRIVATE_KEY_FORMAT, command.PartnerId);
            var passName = string.Format(ResourceLocations.LASO_PGP_PASSPHRASE_FORMAT, command.PartnerId);

            try
            {
                //todo
                //var delPubKey = _secrets.DeleteSecret(pubKeyName, cancellationToken);
                //delPubKey.Wait(cancellationToken);
                //var delPubKeyRec = _tableStorage.DeleteAsync<ProvisionedResourceEvent>(command.PartnerId, $"{ResourceLocations.GetParentLocationByType(ProvisionedResourceType.LasoPGPPublicKey)}-{pubKeyName}");
                //delPubKeyRec.Wait(cancellationToken);
                //var delPrivKey = _secrets.DeleteSecret(privKeyName, cancellationToken);
                //delPrivKey.Wait(cancellationToken);
                //var delPrivKeyRec = _tableStorage.DeleteAsync<ProvisionedResourceEvent>(command.PartnerId, $"{ResourceLocations.GetParentLocationByType(ProvisionedResourceType.LasoPGPPrivateKey)}-{privKeyName}");
                //delPrivKeyRec.Wait(cancellationToken);
                //var delPass = _secrets.DeleteSecret(passName, cancellationToken);
                //delPass.Wait(cancellationToken);
                //var delPassRec = _tableStorage.DeleteAsync<ProvisionedResourceEvent>(command.PartnerId, $"{ResourceLocations.GetParentLocationByType(ProvisionedResourceType.LasoPGPPassphrase)}-{passName}");
                //delPassRec.Wait(cancellationToken);


                var delPubKey = _secrets.DeleteSecret(pubKeyName, cancellationToken);
                delPubKey.Wait(cancellationToken);
                var resource1 = _tableStorage.GetAllAsync<ProvisionedResourceEvent>(x =>
                    x.PartnerId == command.PartnerId && x.Type == ProvisionedResourceType.LasoPGPPublicKey && x.Location == pubKeyName).GetAwaiter().GetResult().MaxBy(x=>x.ProvisionedOn);
                var removeRecord = _tableStorage.DeleteAsync<ProvisionedResourceEvent>(resource1);
                removeRecord.Wait(cancellationToken);
                var delPrivKey = _secrets.DeleteSecret(privKeyName, cancellationToken);
                delPrivKey.Wait(cancellationToken);
                var resource2 = _tableStorage.GetAllAsync<ProvisionedResourceEvent>(x =>
                    x.PartnerId == command.PartnerId && x.Type == ProvisionedResourceType.LasoPGPPrivateKey && x.Location == privKeyName).GetAwaiter().GetResult().MaxBy(x=>x.ProvisionedOn);
                var removeRecord2 = _tableStorage.DeleteAsync<ProvisionedResourceEvent>(resource2);
                removeRecord.Wait(cancellationToken);
                var delPass = _secrets.DeleteSecret(passName, cancellationToken);
                delPass.Wait(cancellationToken);
                var resource3 = _tableStorage.GetAllAsync<ProvisionedResourceEvent>(x =>
                    x.PartnerId == command.PartnerId && x.Type == ProvisionedResourceType.LasoPGPPassphrase && x.Location == passName).GetAwaiter().GetResult().MaxBy(x=>x.ProvisionedOn);
                var removeRecord3 = _tableStorage.DeleteAsync<ProvisionedResourceEvent>(resource3);
                removeRecord.Wait(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not remove at least one element of the pgp key set for {command.PartnerId}.  Resolve the issues and re-issue the command.");
                throw;
            }

            return _bus.Publish(new PartnerPgpKeysDeletedEvent
                {Completed = DateTime.UtcNow, PartnerId = command.PartnerId});
        }
    }
}