using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using Laso.IntegrationEvents;
using Laso.IntegrationMessages;
using Laso.Provisioning.Core.Messaging.SFTP;
using Mono.Unix;
using Mono.Unix.Native;

namespace Provisioning.SFTP.Core.Account
{
    public class CreatePartnerAccountHandler : ICommandHandler<CreatePartnerAccountCommand>
    {
        private readonly IEventPublisher _bus;
        private readonly string _storageAccountName;

        public CreatePartnerAccountHandler(IEventPublisher bus, string storageAccountName)
        {
            _bus = bus;
            _storageAccountName = storageAccountName;
        }

        public Task Handle(CreatePartnerAccountCommand command, CancellationToken cancellationToken)
        {
            var userId = GetUserId(command.AccountName);
            if (userId < 0)
            {
                var createUserTask = CreateUser(command.AccountName, cancellationToken);
                createUserTask.Wait(cancellationToken);
                if (createUserTask.Result != 0)
                    return _bus.Publish(new PartnerAccountCreationFailedEvent
                    {
                        PartnerId = command.PartnerId.ToString(),
                        Completed= DateTime.UtcNow,
                        ErrorMessage = $"Unable to create user {command.AccountName}.  useradd command returned a code of {createUserTask.Result}"
                    });
            }

            var setPasswordTask = SetPassword(command.AccountName, command.Password, cancellationToken);
            setPasswordTask.Wait(cancellationToken);
            if (setPasswordTask.Result != 0)
                return _bus.Publish(new PartnerAccountCreationFailedEvent
                {
                    PartnerId = command.PartnerId.ToString(),
                    Completed= DateTime.UtcNow,
                    ErrorMessage = $"Could not set password for {command.AccountName}.  Exit code was {setPasswordTask.Result}"
                });

            var mountScript = CreateBlobFuseFiles(command.AccountName, command.Container);
            var mountContainerTask = MountContainer(mountScript, cancellationToken);
            mountContainerTask.Wait(cancellationToken);
            if (mountContainerTask.Result != 0)
            {
                return _bus.Publish(new PartnerAccountCreationFailedEvent
                {
                    PartnerId = command.PartnerId.ToString(),
                    Completed= DateTime.UtcNow,
                    ErrorMessage = $"Account was created but was unable to mount {command.Container}.  Script exited with {mountContainerTask.Result}"
                });
            }

            return FinishAccountSetup(command.AccountName, command.PartnerId.ToString());
        }

        private async Task<int> SetPassword(string username, string password, CancellationToken token)
        {
            var cmd = $"{username}:{password}" | Cli.Wrap("/usr/sbin/chpasswd");//TODO: add -e flag if using encrypted password
            var cmdResult = await cmd.ExecuteAsync(token);
            return cmdResult.ExitCode;
        }

        private string CreateBlobFuseFiles(string username, string containerName)
        {
            var confFile = $"/etc/fuse.d/conf/{username}.cfg";
            var mountFile = $"/etc/fuse.d/mount-{username}.sh";
            var rootOwnedDir = $"/home/{username}"; //this will be inaccessible to the partner
            var mountDir = $"/home/{username}/files"; //this will be the lowest accessible directory for the partner
            var tmpDir = $"/tmp/blobfuse/{username}";
            CreateDirectory(rootOwnedDir);
            //TODO: should read these return results (int) and react accordingly
            Syscall.chown(rootOwnedDir, 0, 0); //change owner to root
            var stdPermissions = Get755Permissions();
            Syscall.chmod(rootOwnedDir, stdPermissions);
            CreateDirectory(mountDir);
            Syscall.chmod(mountDir, stdPermissions);
            CreateDirectory(tmpDir);
            Syscall.chmod(tmpDir, stdPermissions);
            CreateBlobfuseConfigFile(confFile, containerName);
            CreateMountScript(mountFile, mountDir, tmpDir, confFile);
            return mountFile;
        }

        private async Task<int> MountContainer(string mountScript, CancellationToken token)
        {
            //TODO: this should test to see if it's already mounted first
            var cmd = Cli.Wrap(mountScript);
            var cmdResult = await cmd.ExecuteAsync(token);
            return cmdResult.ExitCode;
        }

        private void CreateDirectory(string path)
        {
            var directory = new UnixDirectoryInfo(path);
            if(directory.Exists && directory.IsDirectory) return;
            //TODO: what if it exists as a file?
            //TODO: should we allow explicitly setting the owner and permissions in here too?
            directory.Create();
        }

        private Task FinishAccountSetup(string username, string partnerId)
        {
            var mountDir = $"/home/{username}/files"; //this will be the lowest accessible directory for the partner
            var incomingDir = $"{mountDir}/incoming";
            var outgoingDir = $"{mountDir}/outgoing";
            var userId = GetUserId(username);
            if (userId < 0)
                return _bus.Publish(new PartnerAccountCreationFailedEvent
                {
                    Completed = DateTime.UtcNow,
                    ErrorMessage = $"Unable to find user id to finish account setup for {username}",
                    PartnerId = partnerId,
                });
            var userIdAsInt = uint.Parse(userId.ToString());
            CreateDirectory(incomingDir);
            Syscall.chown(incomingDir, userIdAsInt, 0);
            CreateDirectory(outgoingDir);
            Syscall.chown(outgoingDir, userIdAsInt, 0);
            return _bus.Publish(new PartnerAccountCreatedEvent
            {
                Completed = DateTime.UtcNow,
                PartnerId = partnerId
            });
        }

        private FilePermissions Get755Permissions()
        {
            return FilePermissions.S_IRUSR | FilePermissions.S_IWUSR | FilePermissions.S_IXUSR | //7 (full access for owner)
                   FilePermissions.S_IRGRP | FilePermissions.S_IXGRP | //5 (read execute for group)
                   FilePermissions.S_IROTH | FilePermissions.S_IXOTH; //5 (read execute for others)
        }

        private void CreateBlobfuseConfigFile(string path, string containerName)
        {
            var configFile = new UnixFileInfo(path);
            var readOnlyPermissions = FilePermissions.S_IRUSR | FilePermissions.S_IWUSR |  //6 (read write owner)
                                       FilePermissions.S_IRGRP | //4 (read group)
                                       FilePermissions.S_IROTH; //4 (read others)
            //this overwrites if the file exists (which is what we want)
            using (var sw = new StreamWriter(configFile.Create(readOnlyPermissions)))
            {
                sw.WriteLine($"accountName {_storageAccountName}");
                sw.WriteLine("authType MSI");
                sw.WriteLine($"containerName {containerName}");
                sw.Flush();
                sw.Close();
            }
        }

        private void CreateMountScript(string path, string mountDir, string tmpDir, string configFilePath)
        {
            var scriptFile = new UnixFileInfo(path);
            //this overwrites if the file exists (which is what we want)
            using (var sw = new StreamWriter(scriptFile.Create(Get755Permissions())))
            {
                sw.WriteLine("#!/bin/bash");
                sw.WriteLine("PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin");
                sw.WriteLine($"mount.sh {mountDir} {tmpDir} {configFilePath}");
                sw.Flush();
                sw.Close();
            }
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

        private long GetSftpOnlyGroupId()
        {
            try
            {
                var sftpOnly = new UnixGroupInfo("sftp-only");
                return sftpOnly.GroupId;
            }
            catch (ArgumentException)
            {
                throw new SystemException("sftp-only Group has not been created.");
            }
        }

        private async Task<int> CreateUser(string username, CancellationToken token)
        {
            var sftpOnlyGroupId = GetSftpOnlyGroupId();
            var cmd = Cli.Wrap("/usr/sbin/useradd").WithArguments(new[] {"-g", sftpOnlyGroupId.ToString(), username});
            var cmdResult = await cmd.ExecuteAsync(token);
            return cmdResult.ExitCode;
        }
    }
}
