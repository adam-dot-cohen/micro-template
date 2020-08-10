using System.Threading.Tasks;
using Laso.IntegrationEvents;
using Laso.Provisioning.Core.Messaging.SFTP;
using Laso.TableStorage;
using Provisioning.Domain.Entities;

namespace Laso.Provisioning.Infrastructure.SFTP
{
    public class SftpAdminCredentialWatcher : IEventHandler<RotatedAdminPasswordEvent>, IEventHandler<RotateAdminPasswordFailedEvent>
    {
        private readonly ITableStorageService _tableStorage;

        public SftpAdminCredentialWatcher(ITableStorageService tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task Handle(RotatedAdminPasswordEvent @event)
        {
            var credEvent = new SftpAdminCredentialEvent
            {
                On = @event.OnUtc,
                VMInstance = @event.VMInstance,
                Secret = @event.SecretUsedName,
                Version = @event.ToVersion
            };
            return _tableStorage.InsertOrReplaceAsync(credEvent);
        }

        public Task Handle(RotateAdminPasswordFailedEvent @event)
        {
            return _tableStorage.InsertOrReplaceAsync(new SftpAdminCredentialEvent
            {
                On = @event.OnUtc,
                VMInstance = @event.VMInstance,
                Failed = true
            });
        }
    }
}