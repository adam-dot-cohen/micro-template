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
        private readonly IDataQualityPipelineRepository _repository;
        private readonly ILogger<UpdatePipelineRunAddStatusEventHandler> _logger;

        public UpdatePipelineRunAddStatusEventHandler(
            IDataQualityPipelineRepository repository,
            ILogger<UpdatePipelineRunAddStatusEventHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<CommandResponse> Handle(UpdatePipelineRunAddStatusEventCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received DataPipeline Status {@PipelineStatus}", request.Event);
            await _repository.AddPipelineEvent(request.Event);

            return CommandResponse.Succeeded();
        }
    }
}