using Laso.IntegrationEvents;
using Laso.IntegrationMessages;
using Provisioning.Domain.Entities;

namespace Laso.Provisioning.Core.Messaging.AzureResources
{
    public class DeletePartnerDataProcessingDirCommand : CommandMessage
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

    public class PartnerDataProcessingDirDeletedEvent : ProvisioningActionEvent, IIntegrationEvent
    {
        public PartnerDataProcessingDirDeletedEvent()
        {
            Type = ProvisioningActionType.DataProcessingDirectoriesRemoved;
        }
    }
}