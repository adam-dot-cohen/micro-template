using System;

namespace Laso.AdminPortal.Core.IntegrationEvents
{
    public class ProvisioningCompletedEvent
    {
        public DateTime CompletedOn { get; set; }
        public string PartnerId { get; set; }
    }
}