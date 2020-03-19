using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Commands;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Persistence;

namespace Laso.AdminPortal.Infrastructure.Monitoring.DataQualityPipeline.Commands
{
    public class AddEventToPipelineRunHandler : ICommandHandler<AddEventToPipelineRunCommand>
    {
        public async Task<CommandResponse> Handle(AddEventToPipelineRunCommand request, CancellationToken cancellationToken)
        {
            await DataQualityPipelineRepository.AddPipelineEvent(request.Event);

            return CommandResponse.Succeeded();
        }
    }
}