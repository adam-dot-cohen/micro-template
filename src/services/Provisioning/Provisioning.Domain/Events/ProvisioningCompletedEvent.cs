using System;

namespace Provisioning.Domain.Events
{
    public class ProvisioningCompletedEvent
    {
        public DateTime CompletedOn { get; set; }
        public string PartnerId { get; set; }
    }
}
