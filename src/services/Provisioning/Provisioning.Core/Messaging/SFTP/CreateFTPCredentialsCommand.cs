using System;
using Laso.IntegrationEvents;
using Laso.IntegrationMessages;

namespace Laso.Provisioning.Core.Messaging.SFTP
{
    public class CreateFTPCredentialsCommand : CommandMessage
    {
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }

        public override CommandValidationResult ValidateInput(IIntegrationMessage command)
        {
            var result = new CommandValidationResult();
            if(string.IsNullOrWhiteSpace(PartnerId))
                result.AddFailure(nameof(PartnerId),$"You must supply a valid {nameof(PartnerId)}");
            if(string.IsNullOrWhiteSpace(PartnerName))
                result.AddFailure(nameof(PartnerName),$"You must supply a valid {nameof(PartnerName)}");

            return result;
        }
    }
    public class FTPCredentialsCreationFailedEvent : IIntegrationEvent
    {
        public DateTime OnUtc { get; set; }
        public string PartnerId { get; set; }
        public string Reason { get; set; }
    }

    public class FTPCredentialsCreatedEvent : IIntegrationEvent
    {
        public DateTime OnUtc { get; set; }
        public string PartnerId { get; set; }
        public string UsernameSecret { get; set; }
        public string PasswordSecret { get; set; }
    }
}