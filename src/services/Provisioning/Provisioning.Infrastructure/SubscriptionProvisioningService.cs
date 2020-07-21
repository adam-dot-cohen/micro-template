using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Laso.IntegrationEvents;
using Laso.Provisioning.Core;
using Laso.Provisioning.Core.IntegrationEvents;
using Laso.Provisioning.Core.Persistence;
using Microsoft.Extensions.Logging;
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
        private static readonly string[] DataProcessingFilesystems = { "raw", "curated", "rejected", "published", "experiment" };

        private readonly IApplicationSecrets _applicationSecrets;
        private readonly IDataPipelineStorage _dataPipelineStorage;
        private readonly IEscrowBlobStorageService _escrowBlobStorageService;
        private readonly IColdBlobStorageService _coldBlobStorageService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<SubscriptionProvisioningService> _logger;
        
        public SubscriptionProvisioningService(
            IApplicationSecrets applicationSecrets,
            IDataPipelineStorage dataPipelineStorage,
            IEventPublisher eventPublisher, 
            IEscrowBlobStorageService escrowBlobStorageService, 
            IColdBlobStorageService coldBlobStorageService,
            ILogger<SubscriptionProvisioningService> logger)
        {
            _applicationSecrets = applicationSecrets;
            _dataPipelineStorage = dataPipelineStorage;
            _eventPublisher = eventPublisher;
            _escrowBlobStorageService = escrowBlobStorageService;
            _coldBlobStorageService = coldBlobStorageService;
            _logger = logger;
        }

        // TODO: Consider compensating commands for failure conditions.
        public async Task ProvisionPartner(string partnerId, string partnerName, CancellationToken cancellationToken)
        {
            // Create public/private PGP Encryption key pair.
            await CreatePgpKeySet(partnerId, cancellationToken);

            // Create FTP account credentials (this could be queued work, if we have a process manager)
            await CreateFtpCredentials(partnerId, partnerName, cancellationToken);

            // Create Blob Container with incoming and outgoing directories
            await CreateEscrowStorage(partnerId, cancellationToken);

            await CreateColdStorage(partnerId, cancellationToken);

            // TODO: Configure experiment storage? Or wait for data?

            //Configure FTP server with new account
            //(>>> IMPORTANT: The Escrow Storage Command must be successfully completed or this will fail <<<)
            await CreateFtpAccount(partnerId, partnerName, cancellationToken);

            await CreateDataProcessingDirectories(partnerId, cancellationToken);

            // TODO: Ensure/Create partner AD Group
            // TODO: Assign Permissions to provisioned resources (e.g. DataProcessingDirectories)

            // Tell everyone we are done.
            await _eventPublisher.Publish(new ProvisioningCompletedEvent
            {
                CompletedOn = DateTime.UtcNow,
                PartnerId = partnerId
            });

            _logger.LogInformation("Finished provisioning partner.");
        }

        public async Task RemovePartner(string partnerId, CancellationToken cancellationToken)
        {
            await DeleteDataProcessingDirectories(partnerId, cancellationToken);
            await DeleteFTPAccount(partnerId, cancellationToken);
            await DeleteColdStorage(partnerId, cancellationToken);
            await DeleteEscrowStorage(partnerId, cancellationToken);
            await DeleteFtpCredentials(partnerId, cancellationToken);
            await DeletePgpKeySet(partnerId, cancellationToken);
        }

        private Task CreatePgpKeySet(string partnerId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating partner PGP key set.");

            var passPhrase = GetRandomAlphanumericString(10);
            var (publicKey, privateKey) = GenerateKeySet("Laso Insights <insights@laso.com>", passPhrase);

            return Task.WhenAll(
                _applicationSecrets.SetSecret($"{partnerId}-laso-pgp-publickey", publicKey, cancellationToken),
                _applicationSecrets.SetSecret($"{partnerId}-laso-pgp-privatekey", privateKey, cancellationToken),
                _applicationSecrets.SetSecret($"{partnerId}-laso-pgp-passphrase", passPhrase, cancellationToken));
        }

        private Task DeletePgpKeySet(string partnerId, CancellationToken cancellationToken)
        {
            return Task.WhenAll(
                _applicationSecrets.DeleteSecret($"{partnerId}-laso-pgp-publickey", cancellationToken),
                _applicationSecrets.DeleteSecret($"{partnerId}-laso-pgp-privatekey", cancellationToken),
                _applicationSecrets.DeleteSecret($"{partnerId}-laso-pgp-passphrase", cancellationToken));
        }

        // TODO: Make this idempotent -- it is a create, not an update. [jay_mclain]
        private Task CreateFtpCredentials(string partnerId, string partnerName, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating partner FTP credentials.");

            var userName = $"{partnerName}{GetRandomString(4, "0123456789")}";
            var password = GetRandomAlphanumericString(10);

            return Task.WhenAll(
                _applicationSecrets.SetSecret($"{partnerId}-partner-ftp-username", userName, cancellationToken),
                _applicationSecrets.SetSecret($"{partnerId}-partner-ftp-password", password, cancellationToken));
        }

        private Task DeleteFtpCredentials(string partnerId, CancellationToken cancellationToken)
        {
            return Task.WhenAll(
                _applicationSecrets.DeleteSecret($"{partnerId}-partner-ftp-username", cancellationToken),
                _applicationSecrets.DeleteSecret($"{partnerId}-partner-ftp-password", cancellationToken));
        }

        private async Task CreateEscrowStorage(string partnerId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating partner escrow storage.");

            var containerName = GetEscrowContainerName(partnerId);
            await _escrowBlobStorageService.CreateContainerIfNotExists(containerName, cancellationToken);
            await _escrowBlobStorageService.CreateDirectoryIfNotExists(containerName, "incoming", cancellationToken);
            await _escrowBlobStorageService.CreateDirectoryIfNotExists(containerName, "outgoing", cancellationToken);
        }

        private Task DeleteEscrowStorage(string partnerId, CancellationToken cancellationToken)
        {
            var containerName = GetEscrowContainerName(partnerId);
            return _escrowBlobStorageService.DeleteContainerIfExists(containerName, cancellationToken);
        }

        private Task CreateColdStorage(string partnerId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating partner cold storage.");

            return _coldBlobStorageService.CreateContainerIfNotExists(partnerId, cancellationToken);
        }

        private Task DeleteColdStorage(string partnerId, CancellationToken cancellationToken)
        {
            return _coldBlobStorageService.DeleteContainerIfExists(partnerId, cancellationToken);
        }
        
        private async Task CreateFtpAccount(string partnerId, string partnerName, CancellationToken cancellationToken)
        {
            var username = await _applicationSecrets.GetSecret($"{partnerId}-partner-ftp-username", cancellationToken);
            var password = await _applicationSecrets.GetSecret($"{partnerId}-partner-ftp-password", cancellationToken);
            var cmdTxt = $"{username}:{password}:::{GetEscrowContainerName(partnerId)}";
            var cmdPath = $"createpartnersftp/create{partnerName}.cmdtxt";

            await _escrowBlobStorageService.ReplaceTextBlob("provisioning", cmdPath, cmdTxt, cancellationToken);
        }

        private Task DeleteFTPAccount(string partnerId, CancellationToken cancellationToken)
        {
            // TBD - Right now, files will get deleted when escrow is removed...but
            // we probably shouldn't rely on that being removed in all situations.
            return Task.CompletedTask;
        }

        private async Task CreateDataProcessingDirectories(string partnerId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating data processing directories.");

            // Ensure file systems are created
            var createFileSystemTasks = DataProcessingFilesystems
                .Select(f => _dataPipelineStorage.CreateFileSystem(f, cancellationToken))
                .ToArray();
            await Task.WhenAll(createFileSystemTasks);

            // Create partner directories
            var createDirectoryTasks = DataProcessingFilesystems
                .Select(f => _dataPipelineStorage.CreateDirectory(f, partnerId, cancellationToken));
            await Task.WhenAll(createDirectoryTasks);
        }

        private Task DeleteDataProcessingDirectories(string partnerId, CancellationToken cancellationToken)
        {
            var deleteDirectoryTasks = DataProcessingFilesystems
                .Select(f => _dataPipelineStorage.CreateDirectory(f, partnerId, cancellationToken));
            return Task.WhenAll(deleteDirectoryTasks);
        }

        private static string GetEscrowContainerName(string partnerId)
        {
            return $"transfer-{partnerId}";
        }

        // TODO: Make this idempotent -- it is a create, not an update. [jay_mclain]
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
