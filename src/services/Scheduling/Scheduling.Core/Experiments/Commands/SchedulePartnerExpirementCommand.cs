using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laso.IntegrationEvents;
using Laso.Mediation;
using Laso.Scheduling.Core.Experiments.Queries;
using Laso.Scheduling.Core.IntegrationEvents.Publish.Scheduling;
using Laso.Scheduling.Core.IntegrationEvents.Subscribe.CustomerData;
using MediatR;

namespace Laso.Scheduling.Core.Experiments.Commands
{
    public class SchedulePartnerExperimentCommand : ICommand
    {
        public SchedulePartnerExperimentCommand(string partnerId, InputBatchAcceptedEventV1 @event)
        {
            PartnerId = partnerId;
            Event = @event;
        }

        public string PartnerId { get; }

        public InputBatchAcceptedEventV1 Event { get; }
    }

    public class SchedulePartnerExperimentHandler : CommandHandler<SchedulePartnerExperimentCommand>
    {
        private const string SchedulingTopicName = "scheduling";
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

            //if (partnerConfig.Result.ExperimentsEnabled)
            //{
                var experimentEvent = BuildEvent(request);
                await _eventPublisher.Publish(experimentEvent, SchedulingTopicName);
            //}

            return Succeeded();
        }

        private static ExperimentRunScheduledEventV1 BuildEvent(SchedulePartnerExperimentCommand request)
        {
            return new ExperimentRunScheduledEventV1(
                request.PartnerId,
                Guid.NewGuid().ToString(),
                request.Event.FileBatchId,
                request.Event.Files.Select(f => 
                    new ExperimentFile(f.Uri)
                    {
                        DataCategory = f.DataCategory
                    }).ToList());
        }
    }
}