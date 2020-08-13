using System;
using Laso.IntegrationEvents;
using Laso.IntegrationMessages;
using Provisioning.Domain.Entities;

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
    public class FTPCredentialsCreationFailedEvent : ProvisioningActionEvent, IIntegrationEvent
    {
        public FTPCredentialsCreationFailedEvent()
        {
            Type = ProvisioningActionType.FTPCredentialsProvisioned;
        }
    }

    public class FTPCredentialsCreatedEvent : ProvisioningActionEvent, IIntegrationEvent
    {
        public string UsernameSecret { get; set; }
        public string PasswordSecret { get; set; }

        public FTPCredentialsCreatedEvent()
        {
            Type = ProvisioningActionType.FTPCredentialsProvisioned;
        }
    }
}