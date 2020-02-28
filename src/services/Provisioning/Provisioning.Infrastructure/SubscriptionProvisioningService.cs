using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Laso.Provisioning.Core;
using Laso.Provisioning.Domain.Events;

namespace Laso.Provisioning.Infrastructure
{
    public class SubscriptionProvisioningService : ISubscriptionProvisioningService
    {
        private readonly IKeyVaultService _keyVaultService;
        private readonly IEventPublisher _eventPublisher;

        public SubscriptionProvisioningService(IKeyVaultService keyVaultService, IEventPublisher eventPublisher)
        {
            _keyVaultService = keyVaultService;
            _eventPublisher = eventPublisher;
        }

        public async Task ProvisionPartner(string partnerId, string partnerName, CancellationToken cancellationToken)
        {
            // Create FTP account credentials (this could be queued work, if we have a process manager)
            await CreateFtpCredentialsCommand(partnerId, partnerName, cancellationToken);

            // Create public/private PGP Encryption key pair.
            await CreatePgpKeySetCommand(partnerId, cancellationToken);

            // Tell everyone we are done.
            await _eventPublisher.Publish(new ProvisioningCompletedEvent
            {
                CompletedOn = DateTime.UtcNow,
                PartnerId = partnerId
            });
        }

        // TODO: Make this idempotent -- it is a create, not an update. [jay_mclain]
        // TODO: Consider "AwaitAll" on SetSecret tasks. [jay_mclain]
        public async Task CreateFtpCredentialsCommand(string partnerId, string partnerName, CancellationToken cancellationToken)
        {
            var userName = $"{partnerName}{GetRandomString(4, "0123456789")}";
            var password = GetRandomAlphanumericString(10);

            await _keyVaultService.SetSecret($"{partnerId}-partner-ftp-username", userName, cancellationToken);
            await _keyVaultService.SetSecret($"{partnerId}-partner-ftp-password", password, cancellationToken);
        }

        // TODO: Make this idempotent -- it is a create, not an update. [jay_mclain]
        // TODO: Consider "AwaitAll" on SetSecret tasks. [jay_mclain]
        public async Task CreatePgpKeySetCommand(string partnerId, CancellationToken cancellationToken)
        {
            var publicKey = string.Empty;
            var privateKey = string.Empty;
            var passPhrase = string.Empty;

            await _keyVaultService.SetSecret($"{partnerId}-laso-pgp-publickey", publicKey, cancellationToken);
            await _keyVaultService.SetSecret($"{partnerId}-laso-pgp-privatekey", privateKey, cancellationToken);
            await _keyVaultService.SetSecret($"{partnerId}-laso-pgp-passphrase", passPhrase, cancellationToken);
        }

        // TODO: Move this into class, injected into "commands". [jay_mclain]
        private static string GetRandomAlphanumericString(int length)
        {
            // TODO: Consider removing "oO0Ii1", etc. from passwords.
            // TODO: Consider including some symbols for passwords.
            const string alphanumericCharacters =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "abcdefghijklmnopqrstuvwxyz" +
                "0123456789";
            return GetRandomString(length, alphanumericCharacters);
        }

        private static string GetRandomString(int length, IEnumerable<char> characterSet)
        {
            if (length < 0)
                throw new ArgumentException("length must not be negative", nameof(length));
            if (length > int.MaxValue / 8) 
                throw new ArgumentException("length is too large", nameof(length));
            if (characterSet == null)
                throw new ArgumentNullException(nameof(characterSet));

            var characterArray = characterSet.Distinct().ToArray();
            if (characterArray.Length == 0)
                throw new ArgumentException("characterSet must not be empty", nameof(characterSet));

            var bytes = new byte[length * 8];
            new RNGCryptoServiceProvider().GetBytes(bytes);
            var result = new char[length];
            for (var i = 0; i < length; i++)
            {
                var value = BitConverter.ToUInt64(bytes, i * 8);
                result[i] = characterArray[value % (uint)characterArray.Length];
            }

            return new string(result);
        }
    }
}
