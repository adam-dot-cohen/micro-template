using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Mediator;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Commands;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Persistence;
using Microsoft.Extensions.Logging;

namespace Laso.AdminPortal.Infrastructure.Monitoring.DataQualityPipeline.Commands
{
    public class UpdateFileBatchToAcceptedHandler : ICommandHandler<UpdateFileBatchToAcceptedCommand>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateFileBatchToAcceptedHandler> _logger;

        public UpdateFileBatchToAcceptedHandler(IMediator mediator, ILogger<UpdateFileBatchToAcceptedHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<CommandResponse> Handle(UpdateFileBatchToAcceptedCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received DataAccepted Status {@PipelineStatus}", request.Event);
            await DataQualityPipelineRepository.AddPipelineEvent(request.Event);

            //TODO: a single implicit run is created here, but one will need to be created per product in future
            await _mediator.Command(new CreatePipelineRunCommand { FileBatchId = request.Event.FileBatchId }, cancellationToken);

            return CommandResponse.Succeeded();
        }
    }
}