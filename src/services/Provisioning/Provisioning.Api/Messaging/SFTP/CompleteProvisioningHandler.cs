using Laso.Provisioning.Core.Messaging.SFTP;
using System.Threading.Tasks;
using Laso.IntegrationEvents;
using Laso.Provisioning.Core.IntegrationEvents;

namespace Laso.Provisioning.Api.Messaging.SFTP
{
    public class CompleteProvisioningHandler : IEventHandler<PartnerAccountCreatedEvent>, IEventHandler<PartnerAccountCreationFailedEvent>
    {
        private readonly IEventPublisher _integrationPublisher;

        public CompleteProvisioningHandler(IEventPublisher integrationPublisher)
        {
            _integrationPublisher = integrationPublisher;
        }

        public async Task Handle(PartnerAccountCreatedEvent @event)
        {
            // Tell everyone we are done.
            await _integrationPublisher.Publish(new ProvisioningCompletedEvent
            {
                CompletedOn = @event.OnUtc,
                PartnerId = @event.PartnerId.ToString()
            });
        }

        //TODO: we ought to respond differently to a failed event but for now this is good enough
        public async Task Handle(PartnerAccountCreationFailedEvent @event)
        {
            // Tell everyone we are done.
            await _integrationPublisher.Publish(new ProvisioningCompletedEvent
            {
                CompletedOn = @event.OnUtc,
                PartnerId = @event.PartnerId.ToString()
            });
        }
    }
}
