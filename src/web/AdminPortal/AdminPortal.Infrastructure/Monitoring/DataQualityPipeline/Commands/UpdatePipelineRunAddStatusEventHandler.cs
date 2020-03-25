using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Commands;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Persistence;
using Microsoft.Extensions.Logging;

namespace Laso.AdminPortal.Infrastructure.Monitoring.DataQualityPipeline.Commands
{
    public class UpdatePipelineRunAddStatusEventHandler : ICommandHandler<UpdatePipelineRunAddStatusEventCommand>
    {
        private readonly ILogger<UpdatePipelineRunAddStatusEventHandler> _logger;

        public UpdatePipelineRunAddStatusEventHandler(ILogger<UpdatePipelineRunAddStatusEventHandler> logger)
        {
            _logger = logger;
        }

        public async Task<CommandResponse> Handle(UpdatePipelineRunAddStatusEventCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received DataPipeline Status {@PipelineStatus}", request.Event);
            await DataQualityPipelineRepository.AddPipelineEvent(request.Event);

            return CommandResponse.Succeeded();
        }
    }
}