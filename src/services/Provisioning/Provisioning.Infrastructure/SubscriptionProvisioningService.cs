using System;
using Laso.Provisioning.Core;
using Laso.Provisioning.Domain.Events;

namespace Laso.Provisioning.Infrastructure
{
    public class SubscriptionProvisioningService : ISubscriptionProvisioningService
    {
        private readonly IEventPublisher _eventPublisher;

        public SubscriptionProvisioningService(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        public void ProvisionNewPartner(string partnerId)
        {
            _eventPublisher.Publish(new ProvisioningCompletedEvent
            {
                CompletedOn = DateTime.UtcNow,
                PartnerId = partnerId
            });
        }
    }
}
