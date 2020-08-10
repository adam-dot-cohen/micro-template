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
    public class DeletePartnerEscrowStorageHandler : ICommandHandler<DeletePartnerEscrowStorageCommand>
    {

        private readonly IEventPublisher _bus;
        private readonly ILogger<CreatePartnerEscrowStorageHandler> _logger;
        private readonly IEscrowBlobStorageService _blobStorage;
        private readonly ITableStorageService _tableStorage;

        public DeletePartnerEscrowStorageHandler(IEventPublisher bus, ILogger<CreatePartnerEscrowStorageHandler> logger, IEscrowBlobStorageService blobStorage, ITableStorageService tableStorage)
        {
            _bus = bus;
            _logger = logger;
            _blobStorage = blobStorage;
            _tableStorage = tableStorage;
        }

        public Task Handle(DeletePartnerEscrowStorageCommand command, CancellationToken cancellationToken)
        {

            _logger.LogInformation($"Removing partner {command.PartnerId} escrow storage.");
            var containerName = StorageResourceNames.GetEscrowContainerName(command.PartnerId);
            try
            {
                var containerTask = _blobStorage.DeleteContainerIfExists(containerName, cancellationToken);
                containerTask.Wait(cancellationToken);
                var rec = _tableStorage.DeleteAsync<ProvisionedResourceEvent>(command.PartnerId,$"{ResourceLocations.GetParentLocationByType(ProvisionedResourceType.EscrowStorage)}-{containerName}");
                rec.Wait(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not complete removing Escrow Container for {command.PartnerId}.  Re-submit command after resolving issues.");
                throw;
            }
            return _bus.Publish(new PartnerEscrowStorageDeletedEvent
                {Completed = DateTime.UtcNow, PartnerId = command.PartnerId});
        }
    }
}