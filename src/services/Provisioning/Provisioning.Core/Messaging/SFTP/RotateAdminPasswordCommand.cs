using System;
using Laso.IntegrationEvents;
using Laso.IntegrationMessages;
using Provisioning.Domain.Entities;

namespace Laso.Provisioning.Core.Messaging.SFTP
{
    public class RotateAdminPasswordCommand : CommandMessage
    {
        public string AdminUserNameSecretName { get; set; }
        public string AdminUserNameVersion { get; set; }
        public string AdminPasswordSecretName { get; set; }
        public string AdminPasswordVersion { get; set; }
        public override CommandValidationResult ValidateInput(IIntegrationMessage command)
        {
            var result = new CommandValidationResult();
            if(string.IsNullOrWhiteSpace(AdminUserNameSecretName))
                result.AddFailure(nameof(AdminUserNameSecretName), $"You must supply a value for {nameof(AdminUserNameSecretName)}");
            if(string.IsNullOrWhiteSpace(AdminPasswordSecretName))
                result.AddFailure(nameof(AdminPasswordSecretName), $"You must supply a value for {nameof(AdminPasswordSecretName)}");

            return result;
        }
    }

    public class RotateAdminPasswordFailedEvent : IIntegrationEvent
    {
        public DateTime OnUtc { get; set; }
        public string VMInstance { get; set; }
        public string Reason { get; set; }
    }

    public class RotatedAdminPasswordEvent : IIntegrationEvent
    {
        public DateTime OnUtc { get; set; }
        public string VMInstance { get; set; }
        public string SecretUsedName { get; set; }
        public string ToVersion { get; set; }

    }
}
