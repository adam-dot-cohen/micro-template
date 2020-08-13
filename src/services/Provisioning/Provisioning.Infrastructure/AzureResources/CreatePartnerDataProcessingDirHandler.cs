using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.IntegrationEvents;
using Laso.IntegrationMessages;
using Laso.Provisioning.Core;
using Laso.Provisioning.Core.Messaging.AzureResources;
using Laso.TableStorage;
using Microsoft.Extensions.Logging;
using Provisioning.Domain.Entities;

namespace Laso.Provisioning.Infrastructure.AzureResources
{
    public class CreatePartnerDataProcessingDirHandler : ICommandHandler<CreatePartnerDataProcessingDirCommand>
    {
        private readonly ILogger<CreatePartnerDataProcessingDirHandler> _logger;
        private readonly IDataPipelineStorage _dataPipelineStorage;
        private readonly ITableStorageService _tableStorage;
        private readonly IEventPublisher _bus;

        public CreatePartnerDataProcessingDirHandler(IDataPipelineStorage dataPipelineStorage, ITableStorageService tableStorage, IEventPublisher bus, ILogger<CreatePartnerDataProcessingDirHandler> logger)
        {
            _dataPipelineStorage = dataPipelineStorage;
            _tableStorage = tableStorage;
            _bus = bus;
            _logger = logger;
        }

        public Task Handle(CreatePartnerDataProcessingDirCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Creating data processing directories for partner {command.PartnerId}.");
            try
            {
                var targets = StorageResourceNames.GetDataResourceNames();
                foreach (var target in targets)
                {
                    var dirTask =
                        _dataPipelineStorage.CreateDirectory(target.Key, command.PartnerId, cancellationToken);
                    dirTask.Wait(cancellationToken);
                    var recTask = _tableStorage.InsertAsync(new ProvisionedResourceEvent
                    {
                        PartnerId = command.PartnerId,
                        Type = target.Value,
                        ParentLocation = target.Key,
                        Location = command.PartnerId,
                        DisplayName = $"{command.PartnerName} {target.Key} Directory",
                        ProvisionedOn = DateTime.UtcNow
                    });
                    recTask.Wait(cancellationToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not create all cold storage directories for {command.PartnerId}.  Resolve issues and re-submit the command.");
                throw;
            }

            return _bus.Publish(new PartnerDataProcessingDirCreatedEvent
                {Completed = DateTime.UtcNow, PartnerId = command.PartnerId});
        }
    }
}