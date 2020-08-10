using Laso.IntegrationEvents;
using Laso.IntegrationMessages;
using Provisioning.Domain.Entities;

namespace Laso.Provisioning.Core.Messaging.AzureResources
{
    public class DeletePartnerEscrowStorageCommand : CommandMessage
    {
        public string PartnerId { get; set; }
        public override CommandValidationResult ValidateInput(IIntegrationMessage command)
        {
            var result = new CommandValidationResult();

            if(string.IsNullOrWhiteSpace(PartnerId))
                result.AddFailure(nameof(PartnerId),$"You must supply a valid {nameof(PartnerId)}");

            return result;
        }
    }

    public class PartnerEscrowStorageDeletedEvent : ProvisioningActionEvent, IIntegrationEvent
    {
        public PartnerEscrowStorageDeletedEvent()
        {
            Type = ProvisioningActionType.EscrowStorageRemoved;
        }
    }
}