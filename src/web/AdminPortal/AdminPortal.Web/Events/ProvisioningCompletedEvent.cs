using System;

namespace Laso.AdminPortal.Web.Events
{
    public class ProvisioningCompletedEvent
    {
        public DateTime CompletedOn { get; set; }
        public string PartnerId { get; set; }
    }
}