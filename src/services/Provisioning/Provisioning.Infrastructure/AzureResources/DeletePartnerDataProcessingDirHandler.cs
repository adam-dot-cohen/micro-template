using System;
using System.Linq;
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
    public class DeletePartnerDataProcessingDirHandler : ICommandHandler<DeletePartnerDataProcessingDirCommand>
    {

        private readonly ILogger<CreatePartnerDataProcessingDirHandler> _logger;
        private readonly IDataPipelineStorage _dataPipelineStorage;
        private readonly ITableStorageService _tableStorage;
        private readonly IEventPublisher _bus;

        public DeletePartnerDataProcessingDirHandler(ILogger<CreatePartnerDataProcessingDirHandler> logger, IDataPipelineStorage dataPipelineStorage, ITableStorageService tableStorage, IEventPublisher bus)
        {
            _logger = logger;
            _dataPipelineStorage = dataPipelineStorage;
            _tableStorage = tableStorage;
            _bus = bus;
        }

        public Task Handle(DeletePartnerDataProcessingDirCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Removing data processing directories for partner {command.PartnerId}");
            try
            {
                var targets = StorageResourceNames.GetDataResourceNames();
                foreach (var target in targets)
                {
                    var dirTask =
                        _dataPipelineStorage.DeleteDirectory(target.Key, command.PartnerId, cancellationToken);
                    dirTask.Wait(cancellationToken);

                    //var delTask = _tableStorage.DeleteAsync<ProvisionedResourceEvent>(command.PartnerId, $"{target.Key}-{command.PartnerId}");                    
                    //delTask.Wait(cancellationToken);

                    var resource1 = _tableStorage.GetAllAsync<ProvisionedResourceEvent>(x =>
                        x.PartnerId == command.PartnerId && x.Type == target.Value).GetAwaiter().GetResult().MaxBy(x=>x.ProvisionedOn);
                    var removeRecord = _tableStorage.DeleteAsync<ProvisionedResourceEvent>(resource1);
                    removeRecord.Wait(cancellationToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not remove all data processing directories for {command.PartnerId}. Resolve issues and re-submit the command.");
                throw;
            }
            return _bus.Publish(new PartnerDataProcessingDirDeletedEvent
                {Completed = DateTime.UtcNow, PartnerId = command.PartnerId});
        }
    }
}