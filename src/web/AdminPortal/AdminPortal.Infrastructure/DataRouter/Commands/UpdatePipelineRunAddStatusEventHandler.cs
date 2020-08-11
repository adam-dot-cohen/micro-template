using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.DataRouter.Commands;
using Laso.AdminPortal.Core.DataRouter.Persistence;
using Laso.Mediation;
using Microsoft.Extensions.Logging;

namespace Laso.AdminPortal.Infrastructure.DataRouter.Commands
{
    public class UpdatePipelineRunAddStatusEventHandler : CommandHandler<UpdatePipelineRunAddStatusEventCommand>
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

        public override async Task<CommandResponse> Handle(UpdatePipelineRunAddStatusEventCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received DataPipeline Status {@PipelineStatus}", request.Event);
            await _repository.AddPipelineEvent(request.Event);

            return Succeeded();
        }
    }
}