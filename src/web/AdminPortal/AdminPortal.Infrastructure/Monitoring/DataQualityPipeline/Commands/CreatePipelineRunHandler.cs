using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Commands;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Domain;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Persistence;

namespace Laso.AdminPortal.Infrastructure.Monitoring.DataQualityPipeline.Commands
{
    public class CreatePipelineRunHandler : ICommandHandler<CreatePipelineRunCommand, string>
    {
        public async Task<CommandResponse<string>> Handle(CreatePipelineRunCommand request, CancellationToken cancellationToken)
        {
            var fileBatch = await DataQualityPipelineRepository.GetFileBatch(request.FileBatchId);

            var pipelineRun = new PipelineRun
            {
                Id = request.FileBatchId, //TODO: making the Id the same as the FileBatchId temporarily until we have the second event
                PartnerId = fileBatch.PartnerId,
                FileBatchId = request.FileBatchId
            };

            await DataQualityPipelineRepository.AddPipelineRun(pipelineRun);

            return CommandResponse.Succeeded(pipelineRun.Id);
        }
    }
}