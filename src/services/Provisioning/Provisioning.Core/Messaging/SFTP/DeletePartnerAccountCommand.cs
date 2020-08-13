using System;
using Laso.IntegrationMessages;

namespace Laso.Provisioning.Core.Messaging.SFTP
{
    public class DeletePartnerAccountCommand : CommandMessage
    {
        public Guid PartnerId { get; set; }
        public string AccountName { get; set; }
        public override CommandValidationResult ValidateInput(IIntegrationMessage command)
        {
            var result = CommandValidationResult.Valid();
            if (PartnerId == default)
                result.AddFailure(nameof(PartnerId), $"{nameof(PartnerId)} must be supplied.");
            if (string.IsNullOrWhiteSpace(AccountName))
                result.AddFailure(nameof(AccountName), $"{nameof(AccountName)} must be supplied");

            return result;
        }
    }
}