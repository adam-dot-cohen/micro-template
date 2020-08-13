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
    public class DeletePartnerColdStorageHandler : ICommandHandler<DeletePartnerColdStorageCommand>
    {

        private readonly ILogger<CreatePartnerColdStorageHandler> _logger;
        private readonly IEventPublisher _bus;
        private readonly IColdBlobStorageService _blobStorage;
        private readonly ITableStorageService _tableStorage;

        public DeletePartnerColdStorageHandler(ILogger<CreatePartnerColdStorageHandler> logger, IEventPublisher bus, IColdBlobStorageService blobStorage, ITableStorageService tableStorage)
        {
            _logger = logger;
            _bus = bus;
            _blobStorage = blobStorage;
            _tableStorage = tableStorage;
        }

        public Task Handle(DeletePartnerColdStorageCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Removing partner {command.PartnerId} cold storage.");
            try
            {
                var cold = _blobStorage.DeleteContainerIfExists(command.PartnerId, cancellationToken);
                cold.Wait(cancellationToken);
                var delRecord = _tableStorage.DeleteAsync<ProvisionedResourceEvent>(command.PartnerId,$"{ResourceLocations.GetParentLocationByType(ProvisionedResourceType.ColdStorage)}-{command.PartnerId}");
                delRecord.Wait(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not remove cold storage container for {command.PartnerId}.  Resolve issues and re-submit command");
                throw;
            }

            return _bus.Publish(new PartnerColdStorageDeletedEvent{Completed = DateTime.UtcNow, PartnerId = command.PartnerId});
        }
    }
}