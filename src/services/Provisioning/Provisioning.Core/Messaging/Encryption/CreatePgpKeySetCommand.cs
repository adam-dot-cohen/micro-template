using System;
using System.Net.Http.Headers;
using Laso.IntegrationEvents;
using Laso.IntegrationMessages;

namespace Laso.Provisioning.Core.Messaging.Encryption
{
    public class CreatePgpKeySetCommand : CommandMessage
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

    public class PartnerPgpKeySetCreatedEvent : IIntegrationEvent
    {
        public DateTime OnUtc { get; set; }
        public string PartnerId { get; set; }
    }

    public class PartnerPgpKeySetCreationFailedEvent : IIntegrationEvent
    {
        public DateTime OnUtc { get; set; }
        public string PartnerId { get; set; }
        public string Reason { get; set; }
    }
}