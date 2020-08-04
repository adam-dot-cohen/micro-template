using System;
using Laso.IntegrationEvents;
using Laso.IntegrationMessages;

namespace Laso.Provisioning.Core.Messaging.SFTP
{
    public class CreatePartnerAccountCommand : CommandMessage
    {
        public Guid PartnerId { get; set; }
        public string AccountName { get; set; }
        public string Password { get; set; } //TODO: encrypt this
        public string Container { get; set; }

        public override CommandValidationResult ValidateInput(IIntegrationMessage command)
        {
            var result = CommandValidationResult.Valid();

            if (PartnerId == default)
                result.AddFailure(nameof(PartnerId), $"{nameof(PartnerId)} must be supplied.");

            if (string.IsNullOrWhiteSpace(AccountName))
                result.AddFailure(nameof(AccountName), $"{nameof(AccountName)} must be supplied.");
            if (AccountName.Equals("root") || AccountName.Equals("provisioning"))
                result.AddFailure(nameof(AccountName), $"root and provisioning are reserved account names.");

            //TODO: this should be a regex pattern following the rules for passwords
            if (string.IsNullOrWhiteSpace(Password))
                result.AddFailure(nameof(Password), $"{nameof(Password)} must be supplied.");

            if (string.IsNullOrWhiteSpace(Container))
                result.AddFailure(nameof(Container), $"{nameof(Container)} must be supplied.");

            return result;
        }
    }

    public class PartnerAccountCreatedEvent : IIntegrationEvent
    {
        public DateTime OnUtc { get; set; }
        public Guid PartnerId { get; set; }
    }

    public class PartnerAccountCreationFailedEvent : IIntegrationEvent
    {
        public DateTime OnUtc { get; set; }
        public Guid PartnerId { get; set; }
        public string ErrorMessage { get; set;}
    }

    public class PartnerAccountDeletedEvent : IIntegrationEvent
    {
        public DateTime OnUtc { get; set; }
        public Guid PartnerId { get; set; }
    }

    public class DeletePartnerAccountFailedEvent : IIntegrationEvent
    {
        public DateTime OnUtc { get; set; }
        public Guid PartnerId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
