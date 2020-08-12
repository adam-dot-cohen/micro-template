using System.Threading;
using System.Threading.Tasks;
using Laso.IntegrationEvents;
using Laso.Mediation;
using Laso.Scheduling.Core.Experiments.Queries;
using Laso.Scheduling.Core.IntegrationEvents;
using MediatR;

namespace Laso.Scheduling.Core.Experiments.Commands
{
    public class SchedulePartnerExperimentCommand : ICommand
    {
        public SchedulePartnerExperimentCommand(string partnerId)
        {
            PartnerId = partnerId;
        }

        public string PartnerId { get; }
        
    }

    public class SchedulePartnerExperimentHandler : CommandHandler<SchedulePartnerExperimentCommand>
    {
        private readonly IMediator _mediator;
        private readonly IEventPublisher _eventPublisher;

        public SchedulePartnerExperimentHandler(IMediator mediator, IEventPublisher eventPublisher)
        {
            _mediator = mediator;
            _eventPublisher = eventPublisher;
        }

        public override async Task<CommandResponse> Handle(SchedulePartnerExperimentCommand request, CancellationToken cancellationToken)
        {
            var partnerConfigRequest = new GetPartnerExperimentConfigurationQuery(request.PartnerId);
            var partnerConfig = await _mediator.Send(partnerConfigRequest, cancellationToken);

            if (!partnerConfig.Success)
            {
                return partnerConfig.ToResponse<CommandResponse>();
            }

            if (partnerConfig.Result.ExperimentsEnabled)
            {
                var experimentEvent = new ExperimentRunScheduledEventV1(request.PartnerId);
                await _eventPublisher.Publish(experimentEvent, "scheduling");
            }

            return Succeeded();
        }
    }
}