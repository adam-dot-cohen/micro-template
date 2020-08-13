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
    public class CreatePartnerColdStorageHandler : ICommandHandler<CreatePartnerColdStorageCommand>
    {
        private readonly ILogger<CreatePartnerColdStorageHandler> _logger;
        private readonly IEventPublisher _bus;
        private readonly IColdBlobStorageService _blobStorage;
        private readonly ITableStorageService _tableStorage;

        public CreatePartnerColdStorageHandler(ILogger<CreatePartnerColdStorageHandler> logger, IEventPublisher bus, IColdBlobStorageService blobStorage, ITableStorageService tableStorage)
        {
            _logger = logger;
            _bus = bus;
            _blobStorage = blobStorage;
            _tableStorage = tableStorage;
        }

        public Task Handle(CreatePartnerColdStorageCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Creating partner {command.PartnerId} cold storage.");
            try
            {
                var cold = _blobStorage.CreateContainer(command.PartnerId, cancellationToken);
                cold.Wait(cancellationToken);
                var record = _tableStorage.InsertAsync(new ProvisionedResourceEvent
                {
                    PartnerId = command.PartnerId,
                    Type = ProvisionedResourceType.ColdStorage,
                    ParentLocation = ResourceLocations.GetParentLocationByType(ProvisionedResourceType.ColdStorage),
                    Location = command.PartnerId,
                    DisplayName = $"{command.PartnerName} Cold Storage Container",
                    ProvisionedOn = DateTime.UtcNow
                });
                record.Wait(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not create cold storage container for {command.PartnerId}.  Resolve issues and re-submit command");
                throw;
            }

            return _bus.Publish(new PartnerColdStorageCreatedEvent {Completed = DateTime.UtcNow, PartnerId = command.PartnerId});
        }
    }
}