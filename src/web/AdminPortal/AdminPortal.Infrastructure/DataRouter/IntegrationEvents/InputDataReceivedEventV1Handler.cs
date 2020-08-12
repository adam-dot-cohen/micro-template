using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.DataRouter.Commands;
using Laso.Mediation;
using MediatR;

namespace Laso.AdminPortal.Infrastructure.DataRouter.IntegrationEvents
{
    public class InputDataReceivedEventV1Handler : EventHandler<InputDataReceivedEventV1>
    {
        private readonly IMediator _mediator;

        public InputDataReceivedEventV1Handler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override async Task<EventResponse> Handle(InputDataReceivedEventV1 notification, CancellationToken cancellationToken)
        {
            await _mediator.Send(new CreateOrUpdateFileBatchAddFileCommand
            {
                Uri = notification.Uri,
                ETag = notification.ETag,
                ContentType = notification.ContentType,
                ContentLength = notification.ContentLength
            }, cancellationToken);

            return EventResponse.Succeeded();
        }
    }
}