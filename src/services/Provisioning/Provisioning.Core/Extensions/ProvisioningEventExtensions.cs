using Provisioning.Domain.Entities;

namespace Laso.Provisioning.Core.Extensions
{
    public static class ProvisioningEventExtensions
    {
        public static ProvisioningActionEvent CloneToPAE(this ProvisioningActionEvent @event)
        {
            return new ProvisioningActionEvent { PartnerId = @event.PartnerId, Completed = @event.Completed, ErrorMessage = @event.ErrorMessage, Type = @event.Type };
        }
    }
}