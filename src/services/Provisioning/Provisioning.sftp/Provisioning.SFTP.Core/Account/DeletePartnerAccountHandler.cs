using CliWrap;
using CliWrap.Buffered;
using Laso.Provisioning.Core.Messaging.SFTP;
using Mono.Unix;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Laso.IntegrationEvents;
using Laso.IntegrationMessages;

namespace Provisioning.SFTP.Core.Account
{
    public class DeletePartnerAccountHandler : ICommandHandler<DeletePartnerAccountCommand>
    {
        private readonly IEventPublisher _bus;

        public DeletePartnerAccountHandler(IEventPublisher bus)
        {
            _bus = bus;
        }

        public Task Handle(DeletePartnerAccountCommand command, CancellationToken cancellationToken)
        {
            var mountDir = $"/home/{command.AccountName}/files";
            var isMountedCheck = ContainerIsMounted(mountDir, cancellationToken);
            isMountedCheck.Wait(cancellationToken);
            var errors = new StringBuilder();
            if (isMountedCheck.Result)
            {
                var unmountTask = UnMount(mountDir, cancellationToken);
                unmountTask.Wait(cancellationToken);
                if (unmountTask.Result != 0)
                    errors.AppendLine(
                        $"Unable to unmount the container at {mountDir}. Process returned code {unmountTask.Result}");
            }

            var accountTask = RemoveAccount(command.AccountName, cancellationToken);
            accountTask.Wait(cancellationToken);
            if (accountTask.Result != 0)
                errors.AppendLine(
                    $"Unable to remove the {command.AccountName} account.  userdel returned exit code {accountTask.Result}");

            if (errors.Length > 0)
                return _bus.Publish(new DeletePartnerAccountFailedEvent
                {
                    PartnerId = command.PartnerId.ToString(),
                    Completed= DateTime.UtcNow,
                    ErrorMessage = errors.ToString()
                });

            RemoveDirectory($"/tmp/blobfuse/{command.AccountName}");
            RemoveDirectory(mountDir);
            RemoveFile($"/etc/fuse.d/mount-{command.AccountName}.sh");
            RemoveFile($"/etc/fuse.d/conf/{command.AccountName}.cfg");

            return _bus.Publish(new PartnerAccountDeletedEvent
            {
                PartnerId = command.PartnerId.ToString(),
                Completed= DateTime.UtcNow
            });
        }

        private async Task<bool> ContainerIsMounted(string mountDir, CancellationToken token)
        {
            var cmd = Cli.Wrap($"/bin/ps").WithArguments("aux") | Cli.Wrap("/bin/grep").WithArguments(mountDir) |
                      Cli.Wrap("/bin/grep").WithArguments(new[] {"-v", "grep"});

            var cmdOutput = await cmd.ExecuteBufferedAsync(token).Select(r => r.StandardOutput);
            return cmdOutput.Length > 0;
        }

        private async Task<int> UnMount(string mountDir, CancellationToken token)
        {
            var cmd = Cli.Wrap("/bin/fusermount").WithArguments(new[] {"-u", mountDir});
            var cmdResult = await cmd.ExecuteAsync(token);
            return cmdResult.ExitCode;
        }

        private async Task<int> RemoveAccount(string username, CancellationToken token)
        {
            var cmd = Cli.Wrap("/usr/sbin/userdel").WithArguments(new[] {"-f", username});
            var cmdResult = await cmd.ExecuteAsync(token);
            return cmdResult.ExitCode;
        }

        private void RemoveDirectory(string path)
        {
            var targetDir = new UnixDirectoryInfo(path);
            if (!targetDir.Exists) return;
            targetDir.Delete(true);
        }

        private void RemoveFile(string path)
        {
            var targetFile = new UnixFileInfo(path);
            if(!targetFile.Exists) return;
            targetFile.Delete();
        }
    }
}
