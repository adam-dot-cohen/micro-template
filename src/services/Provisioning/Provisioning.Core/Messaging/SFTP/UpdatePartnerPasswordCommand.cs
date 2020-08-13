using System;
using Laso.IntegrationEvents;
using Laso.IntegrationMessages;
using Provisioning.Domain.Entities;

namespace Laso.Provisioning.Core.Messaging.SFTP
{
    public class UpdatePartnerPasswordCommand : CommandMessage
    {
        public Guid PartnerId { get; set; }
        public string Password { get; set; } //TODO: encrypt this
        public string AccountName { get; set; }
        public DateTime AsOfUtc { get; set; }
        public override CommandValidationResult ValidateInput(IIntegrationMessage command)
        {
            var result = CommandValidationResult.Valid();
            if (PartnerId == default)
                result.AddFailure(nameof(PartnerId), $"{nameof(PartnerId)} must be supplied.");
            //TODO: This should be based on password criteria
            if (string.IsNullOrWhiteSpace(Password))
                result.AddFailure(nameof(Password), $"{nameof(Password)} must be supplied.");
            if (AsOfUtc == default)
                result.AddFailure(nameof(AsOfUtc), $"{nameof(AsOfUtc)} must be supplied.");
            return result;
        }
    }

    public class UpdatedPartnerPasswordEvent : ProvisioningActionEvent, IIntegrationEvent
    {
        public UpdatedPartnerPasswordEvent()
        {
            Type = ProvisioningActionType.FTPPasswordUpdated;
        }
    }

    public class FailedToUpdatePartnerPasswordEvent : ProvisioningActionEvent, IIntegrationEvent
    {
        public FailedToUpdatePartnerPasswordEvent()
        {
            Type = ProvisioningActionType.FTPPasswordUpdated;
        }
    }
}