using System;
using Laso.Mediation;

namespace Laso.AdminPortal.Core.IntegrationEvents
{
    public class ProvisioningCompletedEvent : IEvent
    {
        public DateTime CompletedOn { get; set; }
        public string PartnerId { get; set; }
    }
}