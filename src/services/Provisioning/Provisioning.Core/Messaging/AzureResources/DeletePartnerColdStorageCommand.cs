using Laso.IntegrationEvents;
using Laso.IntegrationMessages;
using Provisioning.Domain.Entities;

namespace Laso.Provisioning.Core.Messaging.AzureResources
{
    public class DeletePartnerColdStorageCommand : CommandMessage
    {
        public string PartnerId { get; set; }
        public override CommandValidationResult ValidateInput(IIntegrationMessage command)
        {
            var result = new CommandValidationResult();

            if(string.IsNullOrWhiteSpace(PartnerId))
                result.AddFailure(nameof(PartnerId),$"You must supply a valid {PartnerId}");

            return result;
        }
    }

    public class PartnerColdStorageDeletedEvent : ProvisioningActionEvent, IIntegrationEvent
    {
        public PartnerColdStorageDeletedEvent()
        {
            Type = ProvisioningActionType.ColdStorageRemoved;
        }
    }
}