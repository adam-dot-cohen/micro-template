using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.IntegrationEvents;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Commands;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Domain;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Persistence;

namespace Laso.AdminPortal.Infrastructure.Monitoring.DataQualityPipeline.Commands
{
    public class CreatePipelineRunHandler : ICommandHandler<CreatePipelineRunCommand, string>
    {
        private readonly IDataQualityPipelineRepository _repository;

        public CreatePipelineRunHandler(IDataQualityPipelineRepository repository)
        {
            _repository = repository;
        }

        public async Task<CommandResponse<string>> Handle(CreatePipelineRunCommand request, CancellationToken cancellationToken)
        {
            var fileBatch = await _repository.GetFileBatch(request.FileBatchId);

            var pipelineRun = new PipelineRun
            {
                Id = request.FileBatchId, //TODO: making the Id the same as the FileBatchId temporarily until we have the second event
                PartnerId = fileBatch.PartnerId,
                FileBatchId = request.FileBatchId,
                Files = fileBatch.Files // For now, assume all files go in the new run
            };

            await _repository.AddPipelineRun(pipelineRun);

            var @event = new DataPipelineStatus
            {
                PartnerId = fileBatch.PartnerId,
                Timestamp = DateTimeOffset.UtcNow,
                EventType = "DataPipelineStatus",
                Stage = "Requested",
                CorrelationId = pipelineRun.Id,
                PartnerName = fileBatch.PartnerName
            };
            await _repository.AddPipelineEvent(@event);

            return CommandResponse.Succeeded(pipelineRun.Id);
        }
    }
}