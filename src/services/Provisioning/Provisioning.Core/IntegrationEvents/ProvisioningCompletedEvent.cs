using System;

namespace Laso.Provisioning.Core.IntegrationEvents
{
    public class ProvisioningCompletedEvent : IIntegrationEvent
    {
        public DateTime CompletedOn { get; set; }
        public string PartnerId { get; set; }
    }
}
