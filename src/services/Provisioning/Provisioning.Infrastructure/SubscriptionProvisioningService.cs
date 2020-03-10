using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Laso.Provisioning.Core;
using Laso.Provisioning.Domain.Events;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Laso.Provisioning.Infrastructure
{
    public class SubscriptionProvisioningService : ISubscriptionProvisioningService
    {
        private readonly IApplicationSecrets _secretsService;
        private readonly IEventPublisher _eventPublisher;

        public SubscriptionProvisioningService(IApplicationSecrets secretsService, IEventPublisher eventPublisher)
        {
            _secretsService = secretsService;
            _eventPublisher = eventPublisher;
        }

        public async Task ProvisionPartner(string partnerId, string partnerName, CancellationToken cancellationToken)
        {
            // Create public/private PGP Encryption key pair.
            await CreatePgpKeySetCommand(partnerId, cancellationToken);

            // Create FTP account credentials (this could be queued work, if we have a process manager)
            await CreateFtpCredentialsCommand(partnerId, partnerName, cancellationToken);

            // TODO: Configure incoming storage container

            // TODO: Configure experiment storage? Or wait for data?

            // TODO: Configure FTP server with new account

            // Tell everyone we are done.
            await _eventPublisher.Publish(new ProvisioningCompletedEvent
            {
                CompletedOn = DateTime.UtcNow,
                PartnerId = partnerId
            });
        }

        // TODO: Make this idempotent -- it is a create, not an update. [jay_mclain]
        public Task CreateFtpCredentialsCommand(string partnerId, string partnerName, CancellationToken cancellationToken)
        {
            var userName = $"{partnerName}{GetRandomString(4, "0123456789")}";
            var password = GetRandomAlphanumericString(10);

            return Task.WhenAll(
                _secretsService.SetSecret($"{partnerId}-partner-ftp-username", userName, cancellationToken),
                _secretsService.SetSecret($"{partnerId}-partner-ftp-password", password, cancellationToken));
        }

        // TODO: Make this idempotent -- it is a create, not an update. [jay_mclain]
        public Task CreatePgpKeySetCommand(string partnerId, CancellationToken cancellationToken)
        {
            var passPhrase = GetRandomAlphanumericString(10);
            var (publicKey, privateKey) = GenerateKeySet("Laso Insights <insights@laso.com>", passPhrase);

            return Task.WhenAll(
                _secretsService.SetSecret($"{partnerId}-laso-pgp-publickey", publicKey, cancellationToken),
                _secretsService.SetSecret($"{partnerId}-laso-pgp-privatekey", privateKey, cancellationToken),
                _secretsService.SetSecret($"{partnerId}-laso-pgp-passphrase", passPhrase, cancellationToken));
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

        private static (string publicKey, string privateKey) GenerateKeySet(string username, string passPhrase)
        {
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(new RsaKeyGenerationParameters(BigInteger.ValueOf(65537), new SecureRandom(), 2048, 5));
            var keyPair = keyPairGenerator.GenerateKeyPair();

            var secretKey = new PgpSecretKey(
                PgpSignature.DefaultCertification,
                PublicKeyAlgorithmTag.RsaGeneral,
                keyPair.Public,
                keyPair.Private,
                DateTime.Now,
                username,
                SymmetricKeyAlgorithmTag.Aes256,
                passPhrase.ToCharArray(),
                null,
                null,
                new SecureRandom());

            var privateKeyStream = new MemoryStream();
            var privateKeyOutputStream = new ArmoredOutputStream(privateKeyStream);
            secretKey.Encode(privateKeyOutputStream);
            privateKeyStream.Seek(0, SeekOrigin.Begin);

            var publicKeyStream = new MemoryStream();
            var publicKeyOutputStream = new ArmoredOutputStream(publicKeyStream);
            secretKey.PublicKey.Encode(publicKeyOutputStream);
            publicKeyStream.Seek(0, SeekOrigin.Begin);

            return (GetString(publicKeyStream), GetString(privateKeyStream));
        }

        private static string GetString(Stream stream, Encoding encoding = null)
        {
            if (stream == null)
                return null;

            using (var reader = new StreamReader(stream, encoding ?? Encoding.Default))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
