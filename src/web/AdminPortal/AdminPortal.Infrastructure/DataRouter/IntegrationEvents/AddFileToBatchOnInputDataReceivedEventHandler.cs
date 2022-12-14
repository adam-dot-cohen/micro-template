using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Mediation.Event;
using Laso.AdminPortal.Core.DataRouter.Commands;
using MediatR;

namespace Laso.AdminPortal.Infrastructure.DataRouter.IntegrationEvents
{
    public class AddFileToBatchOnInputDataReceivedEventHandler : IEventHandler<InputDataReceivedEventV1>
    {
        private readonly IMediator _mediator;

        public AddFileToBatchOnInputDataReceivedEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<EventResponse> Handle(InputDataReceivedEventV1 notification, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new CreateOrUpdateFileBatchAddFileCommand
            {
                Uri = notification.Uri,
                ETag = notification.ETag,
                ContentType = notification.ContentType,
                ContentLength = notification.ContentLength
            }, cancellationToken);

            return EventResponse.From(response);
        }
    }
}