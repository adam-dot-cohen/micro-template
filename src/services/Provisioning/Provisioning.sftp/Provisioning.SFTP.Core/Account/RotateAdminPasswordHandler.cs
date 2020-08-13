using System;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using Laso.IntegrationEvents;
using Laso.IntegrationMessages;
using Laso.Provisioning.Core;
using Laso.Provisioning.Core.Messaging.SFTP;
using Mono.Unix;

namespace Laso.Provisioning.SFTP.Core.Account
{
    public class RotateAdminPasswordHandler : ICommandHandler<RotateAdminPasswordCommand>
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IApplicationSecrets _applicationSecrets;
        private readonly string _vmInstance;

        public RotateAdminPasswordHandler(IEventPublisher eventPublisher, IApplicationSecrets applicationSecrets, string vmInstance)
        {
            _eventPublisher = eventPublisher;
            _applicationSecrets = applicationSecrets;
            _vmInstance = vmInstance;
        }

        public Task Handle(RotateAdminPasswordCommand command, CancellationToken cancellationToken)
        {
            Task publishTask;
            //get the secrets
            var getSecretTask = GetSecretSafely(command.AdminUserNameSecretName, command.AdminUserNameVersion,
                cancellationToken);
            getSecretTask.Wait(cancellationToken);
            if (!getSecretTask.Result.Success)
            {
                publishTask = _eventPublisher.Publish(new RotateAdminPasswordFailedEvent{OnUtc = DateTime.UtcNow, VMInstance = _vmInstance, Reason = getSecretTask.Result.ExceptionMessage});
                return publishTask;
            }

            var adminUser = getSecretTask.Result.Secret;
            if (GetUserId(adminUser) < 1)
            {
                publishTask = _eventPublisher.Publish(new RotateAdminPasswordFailedEvent{OnUtc = DateTime.UtcNow, VMInstance = _vmInstance,Reason = $"The user identified in the secret {command.AdminUserNameSecretName}:{command.AdminUserNameVersion} is not a user on this VM."});
                return publishTask;
            }

            getSecretTask = GetSecretSafely(command.AdminPasswordSecretName, command.AdminPasswordVersion,
                cancellationToken);
            getSecretTask.Wait(cancellationToken);
            if (!getSecretTask.Result.Success)
            {
                publishTask = _eventPublisher.Publish(new RotateAdminPasswordFailedEvent{OnUtc = DateTime.UtcNow, VMInstance = _vmInstance, Reason = getSecretTask.Result.ExceptionMessage});
                return publishTask;
            }

            var newPassword = getSecretTask.Result.Secret;
            var updateTask = UpdatePassword(adminUser, newPassword, cancellationToken);
            updateTask.Wait(cancellationToken);
            publishTask = updateTask.Result > 0
                ? _eventPublisher.Publish(new RotatedAdminPasswordEvent
                {
                    OnUtc = DateTime.UtcNow, 
                    VMInstance = _vmInstance,
                    SecretUsedName = command.AdminPasswordSecretName,
                    ToVersion = command.AdminPasswordVersion
                }) 
                : _eventPublisher.Publish(new RotateAdminPasswordFailedEvent{ OnUtc = DateTime.UtcNow, VMInstance = _vmInstance, Reason = "Failed to update password.  Check logs for reason."});

            return publishTask;
        }

        private long GetUserId(string username)
        {
            try
            {
                var uInfo = new UnixUserInfo(username);
                return uInfo.UserId;
            }
            catch (ArgumentException)
            {
                //doesn't exist
                return -1;
            }
        }

        private async Task<SecretResult> GetSecretSafely(string secret, string version, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                try
                {
                    var secretValue = await _applicationSecrets.GetSecret(secret, token);
                    return string.IsNullOrWhiteSpace(secretValue) ? 
                        new SecretResult{Success = false, ExceptionMessage = $"No value found for {secret}."} 
                        : new SecretResult{Success = true, Secret = secretValue};
                }
                catch (Exception e)
                {
                    return new SecretResult {Success = false, ExceptionMessage = e.Message};
                }
            }

            try
            {
                var secretValue = await _applicationSecrets.GetSecret(secret, version, token);
                    return string.IsNullOrWhiteSpace(secretValue) ? 
                        new SecretResult{Success = false, ExceptionMessage = $"No value found for version {version} of secret {secret}."} 
                        : new SecretResult{Success = true, Secret = secretValue};
            }
            catch (Exception e)
            {
                    return new SecretResult {Success = false, ExceptionMessage = e.Message};
            }
        }

        private async Task<int> UpdatePassword(string username, string newPassword, CancellationToken token)
        {
            var cmd = $"{username}:{newPassword}" | Cli.Wrap("/usr/sbin/chpasswd");
            var cmdResult = await cmd.ExecuteAsync(token);
            return cmdResult.ExitCode;
        }

        class SecretResult
        {
            public bool Success { get; set; }
            public string ExceptionMessage { get; set; }
            public string Secret { get; set; }
        }
    }
}
