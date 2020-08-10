using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.IntegrationEvents;
using Laso.IntegrationMessages;
using Laso.Provisioning.Core.Messaging.AzureResources;
using Laso.Provisioning.Core.Persistence;
using Laso.TableStorage;
using Microsoft.Extensions.Logging;
using Provisioning.Domain.Entities;

namespace Laso.Provisioning.Infrastructure.AzureResources
{
    public class CreatePartnerEscrowStorageHandler : ICommandHandler<CreatePartnerEscrowStorageCommand>
    {
        private readonly IEventPublisher _bus;
        private readonly ILogger<CreatePartnerEscrowStorageHandler> _logger;
        private readonly IEscrowBlobStorageService _blobStorage;
        private readonly ITableStorageService _tableStorage;

        public CreatePartnerEscrowStorageHandler(IEventPublisher bus, ILogger<CreatePartnerEscrowStorageHandler> logger, IEscrowBlobStorageService blobStorage, ITableStorageService tableStorage)
        {
            _bus = bus;
            _logger = logger;
            _blobStorage = blobStorage;
            _tableStorage = tableStorage;
        }

        public Task Handle(CreatePartnerEscrowStorageCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating partner escrow storage.");
            var containerName = StorageResourceNames.GetEscrowContainerName(command.PartnerId);
            try
            {
                var containerMaker = _blobStorage.CreateContainer(containerName, cancellationToken);
                containerMaker.Wait(cancellationToken);
                var rec = _tableStorage.InsertAsync(new ProvisionedResourceEvent
                    {
                        PartnerId = command.PartnerId,
                        Type = ProvisionedResourceType.EscrowStorage,
                        ParentLocation =
                            ResourceLocations.GetParentLocationByType(ProvisionedResourceType.EscrowStorage),
                        Location = containerName,
                        DisplayName = $"{command.PartnerName} Escrow Container",
                        ProvisionedOn = DateTime.UtcNow
                    });
                rec.Wait(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not complete creating Escrow Container for {command.PartnerId}.  Re-submit command after resolving issues.");
                throw;
            }

            return _bus.Publish(new EscrowPartnerStorageCreatedEvent {Completed = DateTime.UtcNow,PartnerId = command.PartnerId});
        }
    }
}