using System;
using Infrastructure.Mediation.Event;

namespace Laso.AdminPortal.Core.IntegrationEvents
{
    public class ProvisioningCompletedEvent : IEvent
    {
        public DateTime CompletedOn { get; set; }
        public string PartnerId { get; set; }
    }
}