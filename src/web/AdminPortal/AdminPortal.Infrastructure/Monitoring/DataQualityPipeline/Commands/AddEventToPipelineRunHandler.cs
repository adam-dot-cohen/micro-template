using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Commands;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Persistence;
using Microsoft.Extensions.Logging;

namespace Laso.AdminPortal.Infrastructure.Monitoring.DataQualityPipeline.Commands
{
    public class AddEventToPipelineRunHandler : ICommandHandler<AddEventToPipelineRunCommand>
    {
        private readonly ILogger<AddEventToPipelineRunHandler> _logger;

        public AddEventToPipelineRunHandler(ILogger<AddEventToPipelineRunHandler> logger)
        {
            _logger = logger;
        }

        public async Task<CommandResponse> Handle(AddEventToPipelineRunCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received DataPipeline Status {@PipelineStatus}", request.Event);
            await DataQualityPipelineRepository.AddPipelineEvent(request.Event);

            return CommandResponse.Succeeded();
        }
    }
}