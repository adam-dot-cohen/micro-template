using CliWrap;
using Laso.Provisioning.Core.Messaging.SFTP;
using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.IntegrationEvents;
using Laso.IntegrationMessages;
using Mono.Unix;

namespace Provisioning.SFTP.Core.Account
{
    public class UpdatePartnerPasswordHandler : ICommandHandler<UpdatePartnerPasswordCommand>
    {
        private readonly IEventPublisher _bus;

        public UpdatePartnerPasswordHandler(IEventPublisher bus)
        {
            _bus = bus;
        }

        public Task Handle(UpdatePartnerPasswordCommand command, CancellationToken cancellationToken)
        {
            //TODO: use /usr/sbin/passwd -S $username to check when the password was last changed against command.AsOfUtc and exit if newer
            if (GetUserId(command.AccountName) < 0)
                return _bus.Publish(new FailedToUpdatePartnerPasswordEvent
                {
                    ErrorMessage = $"No account found for {command.AccountName}",
                    Completed = DateTime.UtcNow,
                    PartnerId = command.PartnerId.ToString()
                });

            var updateTask = UpdatePassword(command.AccountName, command.Password, cancellationToken);
            updateTask.Wait(cancellationToken);
            if (updateTask.Result != 0)
                return _bus.Publish(new FailedToUpdatePartnerPasswordEvent
                {
                    PartnerId = command.PartnerId.ToString(), 
                    Completed = DateTime.UtcNow, 
                    ErrorMessage = $"Unable to update password for {command.AccountName}.  chpasswd returned exit code {updateTask.Result}"
                });

            return _bus.Publish(
                new UpdatedPartnerPasswordEvent {PartnerId = command.PartnerId.ToString(), Completed = DateTime.UtcNow});
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

        private async Task<int> UpdatePassword(string username, string newPassword, CancellationToken token)
        {
            var cmd = $"{username}:{newPassword}" | Cli.Wrap("/usr/sbin/chpasswd");
            var cmdResult = await cmd.ExecuteAsync(token);
            return cmdResult.ExitCode;
        }
    }
}
