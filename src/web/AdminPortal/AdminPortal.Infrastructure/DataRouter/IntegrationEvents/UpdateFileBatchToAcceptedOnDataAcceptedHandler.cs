using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Mediation.Event;
using Laso.AdminPortal.Core.DataRouter.Commands;
using Laso.AdminPortal.Core.IntegrationEvents;
using MediatR;

namespace Laso.AdminPortal.Infrastructure.DataRouter.IntegrationEvents
{
    public class UpdateFileBatchToAcceptedOnDataAcceptedHandler : IEventHandler<DataPipelineStatus>
    {
        private readonly IMediator _mediator;

        public UpdateFileBatchToAcceptedOnDataAcceptedHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<EventResponse> Handle(DataPipelineStatus notification, CancellationToken cancellationToken)
        {
            if (notification.EventType != "DataAccepted")
                return EventResponse.Succeeded();

            var response = await _mediator.Send(new UpdateFileBatchToAcceptedCommand { Event = notification }, cancellationToken);

            return EventResponse.From(response);
        }
    }

    public class AddStatusEventOnDataPipelineStatusHandler : IEventHandler<DataPipelineStatus>
    {
        private readonly IMediator _mediator;

        public AddStatusEventOnDataPipelineStatusHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<EventResponse> Handle(DataPipelineStatus notification, CancellationToken cancellationToken)
        {
            if (notification.EventType == "DataAccepted")
                return EventResponse.Succeeded();

            var response = await _mediator.Send(new UpdatePipelineRunAddStatusEventCommand { Event = notification }, cancellationToken);

            return EventResponse.From(response);
        }
    }
}