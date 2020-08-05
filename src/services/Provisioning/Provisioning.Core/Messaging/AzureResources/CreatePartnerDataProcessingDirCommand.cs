using System;
using Laso.IntegrationEvents;
using Laso.IntegrationMessages;

namespace Laso.Provisioning.Core.Messaging.AzureResources
{
    public class CreatePartnerDataProcessingDirCommand : CommandMessage
    {
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }

        public override CommandValidationResult ValidateInput(IIntegrationMessage command)
        {
            var result = new CommandValidationResult();

            if(string.IsNullOrWhiteSpace(PartnerId))
                result.AddFailure(nameof(PartnerId),$"You must supply a valid {nameof(PartnerId)}");

            return result;
        }
    }

    public class PartnerDataProcessingDirCreatedEvent : IIntegrationEvent
    {
        public DateTime OnUtc { get; set; }
        public string PartnerId { get; set; }
    }

    public class PartnerDataProcessingDirFailedEvent : IIntegrationEvent
    {
        public DateTime OnUtc { get; set; }
        public string PartnerId { get; set; }
        public string Reason { get; set; }
    }
}