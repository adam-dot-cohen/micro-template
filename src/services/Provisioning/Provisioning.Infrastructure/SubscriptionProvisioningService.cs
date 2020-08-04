using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Laso.IntegrationMessages;
using Laso.Provisioning.Core;
using Laso.Provisioning.Core.Messaging.SFTP;
using Laso.Provisioning.Core.Persistence;
using Laso.TableStorage;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Provisioning.Domain.Entities;

namespace Laso.Provisioning.Infrastructure
{
    public class SubscriptionProvisioningService : ISubscriptionProvisioningService
    {
        private readonly IApplicationSecrets _applicationSecrets;
        private readonly IDataPipelineStorage _dataPipelineStorage;
        private readonly IEscrowBlobStorageService _escrowBlobStorageService;
        private readonly IColdBlobStorageService _coldBlobStorageService;
        private readonly IMessageSender _bus;
        private readonly ILogger<SubscriptionProvisioningService> _logger;
        private readonly ITableStorageService _tableStorageService;

        private const string EscrowStorage = "EscrowStorage";
        private const string ColdStorage = "ColdStorage";
        private const string ActiveDirectory = "AD";

        private Dictionary<string, ProvisionedResourceType> DATA_RESOURCES = new Dictionary<string, ProvisionedResourceType>
        {
                {"raw", ProvisionedResourceType.RawFileSystemDirectory},
                {"curated", ProvisionedResourceType.CuratedFileSystemDirectory},
                {"rejected", ProvisionedResourceType.RejectedFileSystemDirectory},
                {"published", ProvisionedResourceType.PublishedFileSystemDirectory},
                {"experiment", ProvisionedResourceType.ExperimentFileSystemDirectory}
        };
        private const string SFTPVM = "sFTP";
        
        public SubscriptionProvisioningService(
            IApplicationSecrets applicationSecrets,
            IDataPipelineStorage dataPipelineStorage,
            IMessageSender bus,
            IEscrowBlobStorageService escrowBlobStorageService, 
            IColdBlobStorageService coldBlobStorageService,
            ILogger<SubscriptionProvisioningService> logger, ITableStorageService tableStorageService)
        {
            _applicationSecrets = applicationSecrets;
            _dataPipelineStorage = dataPipelineStorage;
            _bus = bus;
            _escrowBlobStorageService = escrowBlobStorageService;
            _coldBlobStorageService = coldBlobStorageService;
            _logger = logger;
            _tableStorageService = tableStorageService;
        }

        // TODO: Consider compensating commands for failure conditions.
        public async Task ProvisionPartner(string partnerId, string partnerName, CancellationToken cancellationToken)
        {
            // Create public/private PGP Encryption key pair.
            await CreatePgpKeySetCommand(partnerId, cancellationToken);

            // Create FTP account credentials (this could be queued work, if we have a process manager)
            await CreateFtpCredentialsCommand(partnerId, partnerName, cancellationToken);

            // Create Blob Container with incoming and outgoing directories
            await CreateEscrowStorageCommand(partnerId, partnerName, cancellationToken);

            await CreateColdStorageCommand(partnerId, partnerName, cancellationToken);

            // TODO: Configure experiment storage? Or wait for data?

            await CreateDataProcessingDirectoriesCommand(partnerId, partnerName, cancellationToken);

            // TODO: Ensure/Create partner AD Group
            // TODO: Assign Permissions to provisioned resources (e.g. DataProcessingDirectories)

            //Configure FTP server with new account
            //(>>> IMPORTANT: The Escrow Storage Command must be successfully completed or this will fail <<<)
            await CreateFTPAccountCommand(partnerId, partnerName, cancellationToken);

            _logger.LogInformation($"Finished initial provisioning of partner {partnerId}.");
        }

        public async Task RemovePartner(string partnerId, CancellationToken cancellationToken)
        {
            await DeleteDataProcessingDirectories(partnerId, cancellationToken);
            await DeleteColdStorage(partnerId, cancellationToken);
            await DeleteEscrowStorage(partnerId, cancellationToken);
            await DeleteFtpCredentials(partnerId, cancellationToken);
            await DeletePgpKeySetCommand(partnerId, cancellationToken);
            await DeleteFTPAccount(partnerId, cancellationToken);
            _logger.LogInformation($"Finished initial de-provisioning of partner {partnerId}.");
        }

        private Task CreatePgpKeySetCommand(string partnerId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating partner PGP key set.");

            var pubKeyName = string.Format(ResourceLocations.LASO_PGP_PUBLIC_KEY_FORMAT, partnerId);
            var privKeyName = string.Format(ResourceLocations.LASO_PGP_PRIVATE_KEY_FORMAT, partnerId);
            var passName = string.Format(ResourceLocations.LASO_PGP_PASSPHRASE_FORMAT, partnerId);
            var pEvent = new ProvisioningActionEvent
            {
                PartnerId = partnerId,
                Type = ProvisioningActionType.PGPKeysetProvisioned,
                Started = DateTime.UtcNow
            };

            var passPhrase = GetRandomAlphanumericString(10);
            var (publicKey, privateKey) = GenerateKeySet("Laso Insights <insights@laso.com>", passPhrase);

            try
            {
                return Task.WhenAll(
                    _applicationSecrets.SetSecret(pubKeyName, publicKey, cancellationToken),
                    _tableStorageService.InsertAsync(new ProvisionedResourceEvent
                    {
                        PartnerId = partnerId,
                        Type = ProvisionedResourceType.LasoPGPPublicKey,
                        ParentLocation = ResourceLocations.GetParentLocationByType(ProvisionedResourceType.LasoPGPPublicKey),
                        Location = pubKeyName,
                        DisplayName = "Laso Public Key",
                        ProvisionedOn = DateTime.UtcNow,
                        Sensitive = true
                    }),
                    _applicationSecrets.SetSecret(privKeyName, privateKey, cancellationToken),
                    _tableStorageService.InsertAsync(new ProvisionedResourceEvent
                    {
                        PartnerId = partnerId,
                        Type = ProvisionedResourceType.LasoPGPPrivateKey,
                        ParentLocation = ResourceLocations.GetParentLocationByType(ProvisionedResourceType.LasoPGPPrivateKey),
                        Location = privKeyName,
                        DisplayName = "Laso Private Key",
                        ProvisionedOn = DateTime.UtcNow,
                        Sensitive = true
                    }),
                    _applicationSecrets.SetSecret(passName, passPhrase, cancellationToken),
                    _tableStorageService.InsertAsync(new ProvisionedResourceEvent
                    {
                        PartnerId = partnerId,
                        Type = ProvisionedResourceType.LasoPGPPassphrase,
                        ParentLocation = ResourceLocations.GetParentLocationByType(ProvisionedResourceType.LasoPGPPassphrase),
                        Location = passName,
                        DisplayName = "Laso PGP Pass Phrase",
                        ProvisionedOn = DateTime.UtcNow,
                        Sensitive = true
                    })
                ).ContinueWith(t => PersistEvent(pEvent),cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not provision Laso PGP resources for {partnerId}");
                return PersistEvent(pEvent, e);
            }
        }

        private Task DeletePgpKeySetCommand(string partnerId, CancellationToken cancellationToken)
        {
            var pubKeyName = string.Format(ResourceLocations.LASO_PGP_PUBLIC_KEY_FORMAT, partnerId);
            var privKeyName = string.Format(ResourceLocations.LASO_PGP_PRIVATE_KEY_FORMAT, partnerId);
            var passName = string.Format(ResourceLocations.LASO_PGP_PASSPHRASE_FORMAT, partnerId);
            var pEvent = new ProvisioningActionEvent
            {
                PartnerId = partnerId,
                Type = ProvisioningActionType.PGPKeysetRemoved,
                Started = DateTime.UtcNow
            };
            try
            {
                return Task.WhenAll(
                    _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId, $"{ResourceLocations.PARTNERSECRETS}-{pubKeyName}"),
                    _applicationSecrets.DeleteSecret(pubKeyName, cancellationToken),
                    _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId, $"{ResourceLocations.PARTNERSECRETS}-{privKeyName}"),
                    _applicationSecrets.DeleteSecret(privKeyName, cancellationToken),
                    _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId, $"{ResourceLocations.PARTNERSECRETS}-{passName}"),
                    _applicationSecrets.DeleteSecret(passName, cancellationToken))
                    .ContinueWith(t => PersistEvent(pEvent), cancellationToken);

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not remove PGP KeySet for {partnerId}");
                return PersistEvent(pEvent, e);
            }
        }

        private Task CreateFtpCredentialsCommand(string partnerId, string partnerName, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating partner FTP credentials.");
            var pEvent = new ProvisioningActionEvent
            {
                PartnerId = partnerId,
                Type = ProvisioningActionType.FTPCredentialsProvisioned,
                Started = DateTime.UtcNow
            };
            var userNLoc = string.Format(ResourceLocations.SFTP_USERNAME_FORMAT, partnerId);
            var pwLoc = string.Format(ResourceLocations.SFTP_PASSWORD_FORMAT, partnerId);
            var checkStateTask = _tableStorageService.GetAsync<ProvisionedResourceEvent>(partnerId, $"{ResourceLocations.PARTNERSECRETS}-{userNLoc}");
            var userName = $"{partnerName}{GetRandomString(4, "0123456789")}";
            var password = GetRandomAlphanumericString(10);

            try
            {
                return checkStateTask.ContinueWith(c =>
                {
                    if (c.Result != null)
                    {
                        PersistEvent(pEvent,
                            new Exception(
                                $"{partnerName} was already provisioned {checkStateTask.Result.PartitionKey}-{checkStateTask.Result.RowKey}"));
                    }
                    else
                    {
                        Task.WhenAll(
                            _applicationSecrets.SetSecret(userNLoc, userName, cancellationToken),
                            _tableStorageService.InsertAsync(new ProvisionedResourceEvent
                            {
                                PartnerId = partnerId,
                                Type = ProvisionedResourceType.SFTPUsername,
                                ParentLocation =
                                    ResourceLocations.GetParentLocationByType(ProvisionedResourceType.SFTPUsername),
                                Location = userNLoc,
                                DisplayName = "sFTP User Name",
                                ProvisionedOn = DateTime.UtcNow
                            }),
                            _applicationSecrets.SetSecret(pwLoc, password, cancellationToken),
                            _tableStorageService.InsertAsync(new ProvisionedResourceEvent
                            {
                                PartnerId = partnerId,
                                Type = ProvisionedResourceType.SFTPPassword,
                                ParentLocation =
                                    ResourceLocations.GetParentLocationByType(ProvisionedResourceType.SFTPPassword),
                                Location = pwLoc,
                                DisplayName = "sFTP Password",
                                Sensitive = true,
                                ProvisionedOn = DateTime.UtcNow
                            })
                        ).ContinueWith(t => PersistEvent(pEvent), cancellationToken);
                    }
                }, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e,$"Could not complete provisioning of FTP Credentials for {partnerId}");
                return PersistEvent(pEvent, e);
            }
        }

        
        private Task DeleteFtpCredentials(string partnerId, CancellationToken cancellationToken)
        {
            var pEvent = new ProvisioningActionEvent
            {
                PartnerId = partnerId,
                Type = ProvisioningActionType.FTPCredentialsRemoved,
                Started = DateTime.UtcNow
            };
            var userNLoc = string.Format(ResourceLocations.SFTP_USERNAME_FORMAT, partnerId);
            var pwLoc = string.Format(ResourceLocations.SFTP_PASSWORD_FORMAT, partnerId);
            try
            {
                return Task.WhenAll(
                    _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId, $"{ResourceLocations.PARTNERSECRETS}-{userNLoc}"),
                    _applicationSecrets.DeleteSecret(userNLoc, cancellationToken),
                    _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId, $"{ResourceLocations.PARTNERSECRETS}-{pwLoc}"),
                    _applicationSecrets.DeleteSecret(pwLoc, cancellationToken))
                    .ContinueWith(t => PersistEvent(pEvent),cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not remove FTP Credentials for {partnerId}");
                return PersistEvent(pEvent, e);
            }
        }

        public Task CreateEscrowStorageCommand(string partnerId, string partnerName, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating partner escrow storage.");
            var pEvent = new ProvisioningActionEvent
            {
                PartnerId = partnerId,
                Started = DateTime.UtcNow,
                Type = ProvisioningActionType.EscrowStorageProvisioned
            };

            var containerName = GetEscrowContainerName(partnerId);
            var inDirectory = "incoming";
            var outDirectory = "outgoing";
            try
            {
                return Task.WhenAll(
                    _escrowBlobStorageService.CreateContainer(containerName, cancellationToken),
                    _tableStorageService.InsertAsync(new ProvisionedResourceEvent
                    {
                        PartnerId = partnerId,
                        Type = ProvisionedResourceType.EscrowStorage,
                        ParentLocation = ResourceLocations.GetParentLocationByType(ProvisionedResourceType.EscrowStorage),
                        Location = containerName,
                        DisplayName = $"{partnerName} Escrow Container",
                        ProvisionedOn = DateTime.UtcNow
                    }),
                    _escrowBlobStorageService.CreateDirectory(containerName, inDirectory, cancellationToken),
                    _tableStorageService.InsertAsync(new ProvisionedResourceEvent
                    {
                        PartnerId = partnerId,
                        Type = ProvisionedResourceType.EscrowIncoming,
                        ParentLocation = $"{EscrowStorage}-{containerName}", //TODO: this should really be the Rowkey from container
                        Location = inDirectory,
                        DisplayName = $"{partnerName} Incoming Escrow Directory",
                        ProvisionedOn = DateTime.UtcNow
                    }),
                    _escrowBlobStorageService.CreateDirectory(containerName, outDirectory, cancellationToken),
                    _tableStorageService.InsertAsync(new ProvisionedResourceEvent
                    {
                        PartnerId = partnerId,
                        Type = ProvisionedResourceType.EscrowOutgoing,
                        ParentLocation = $"{EscrowStorage}-{containerName}", //TODO: this should really be the Rowkey from container
                        Location = outDirectory,
                        DisplayName = $"{partnerName} Outgoing Escrow Directory",
                        ProvisionedOn = DateTime.UtcNow
                    })
                ).ContinueWith(t => PersistEvent(pEvent),cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not provision Escrow Storage for {partnerId}");
                return PersistEvent(pEvent, e);
            }
		}

        private Task DeleteEscrowStorage(string partnerId, CancellationToken cancellationToken)
        {
            var containerName = GetEscrowContainerName(partnerId);
            var inDirectory = "incoming";
            var outDirectory = "outgoing";
            var pEvent = new ProvisioningActionEvent
            {
                PartnerId = partnerId,
                Type = ProvisioningActionType.EscrowStorageRemoved,
                Started = DateTime.UtcNow
            };
            try
            {
                return Task.WhenAll(
                        _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId,
                            $"{EscrowStorage}-{containerName}"),
                        _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId,
                            $"{EscrowStorage}-{containerName}-{inDirectory}"),
                        _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId,
                            $"{EscrowStorage}-{containerName}-{outDirectory}"),
                        _escrowBlobStorageService.DeleteContainerIfExists(containerName, cancellationToken)
                        ).ContinueWith(t => PersistEvent(pEvent),cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not remove Escrow Storage for {partnerId}");
                return PersistEvent(pEvent, e);
            }
        }

        public Task CreateColdStorageCommand(string partnerId, string partnerName, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating partner cold storage.");
            var pEvent = new ProvisioningActionEvent
            {
                PartnerId = partnerId,
                Type = ProvisioningActionType.ColdStorageProvisioned,
                Started = DateTime.UtcNow
            };

            try
            {
                return Task.WhenAll(
                    _coldBlobStorageService.CreateContainer(partnerId, cancellationToken),
                    _tableStorageService.InsertAsync(new ProvisionedResourceEvent
                    {
                        PartnerId = partnerId,
                        Type = ProvisionedResourceType.ColdStorage,
                        ParentLocation = ColdStorage,
                        Location = partnerId,
                        DisplayName = $"{partnerName} Cold Storage Container",
                        ProvisionedOn = DateTime.UtcNow
                    })
                ).ContinueWith(t => PersistEvent(pEvent),cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not provision Cold Storage for {partnerId}");
                return PersistEvent(pEvent, e);
            }
        }

        private Task DeleteColdStorage(string partnerId, CancellationToken cancellationToken)
        {
            var pEvent = new ProvisioningActionEvent
            {
                PartnerId = partnerId,
                Type = ProvisioningActionType.ColdStorageRemoved,
                Started = DateTime.UtcNow
            };
            try
            {
                return Task.WhenAll(_coldBlobStorageService.DeleteContainerIfExists(partnerId, cancellationToken),
                    _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId,$"{ColdStorage}-{partnerId}"))
                    .ContinueWith(t => PersistEvent(pEvent),cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not remove Cold Storage for {partnerId}");
                return PersistEvent(pEvent, e);
            }
        }

        public Task CreateFTPAccountCommand(string partnerId, string partnerName, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Started FTP Account provisioning for {partnerId}");
            var pEvent = new ProvisioningActionEvent
            {
                PartnerId = partnerId,
                Type = ProvisioningActionType.FTPAccountProvisioned,
                Started = DateTime.UtcNow
            };

            var getUserNameTask = _applicationSecrets.GetSecret(string.Format(ResourceLocations.SFTP_USERNAME_FORMAT, partnerId), cancellationToken);
            var getPWTask = _applicationSecrets.GetSecret(string.Format(ResourceLocations.SFTP_PASSWORD_FORMAT, partnerId), cancellationToken);

            try
            {
                return Task.WhenAll(getUserNameTask, getPWTask).ContinueWith(o =>
                {
                    if (string.IsNullOrWhiteSpace(getUserNameTask.Result) ||
                        string.IsNullOrWhiteSpace(getPWTask.Result))
                    {
                        PersistEvent(pEvent,
                            new Exception(
                                $"Could not retrieve the sftp username or password for {partnerId}.  Cannot provision sFTP account."));
                    }
                    else
                    {

                        var cmd = new CreatePartnerAccountCommand
                        {
                            PartnerId = Guid.Parse(partnerId),
                            AccountName = getUserNameTask.Result,
                            Password = getPWTask.Result,
                            Container = GetEscrowContainerName(partnerId)
                        };
                        Task.WhenAll(
                            _bus.Send(cmd),
                            _tableStorageService.InsertAsync(new ProvisionedResourceEvent
                            {
                                PartnerId = partnerId,
                                Type = ProvisionedResourceType.SFTPAccount,
                                ParentLocation = SFTPVM,
                                Location = getUserNameTask.Result,
                                ProvisionedOn = DateTime.UtcNow
                            })
                        ).ContinueWith(t => PersistEvent(pEvent), cancellationToken);
                    }
                },cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not provision {partnerId}");

                return PersistEvent(pEvent, e);
            }
        }

        private Task DeleteFTPAccount(string partnerId, CancellationToken cancellationToken)
        {
            var pEvent = new ProvisioningActionEvent
            {
                PartnerId = partnerId,
                Started = DateTime.UtcNow,
                Type = ProvisioningActionType.FTPAccountRemoved
            };
            var getUserNameTask = _applicationSecrets.GetSecret(string.Format(ResourceLocations.SFTP_USERNAME_FORMAT, partnerId), cancellationToken);
            try
            {
                return getUserNameTask.ContinueWith(ut =>
                {
                    if (string.IsNullOrWhiteSpace(getUserNameTask.Result))
                    {
                        PersistEvent(pEvent, new ArgumentOutOfRangeException(
                            $"No user name found for {partnerId}.  You may have to issue the command manually."));
                    }
                    else
                    {
                        var cmd = new DeletePartnerAccountCommand
                        {
                            PartnerId = Guid.Parse(partnerId),
                            AccountName = getUserNameTask.Result
                        };
                        Task.WhenAll(
                                _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId,
                                    $"{SFTPVM}-{getUserNameTask.Result}"),
                                _bus.Send(cmd))
                            .ContinueWith(t => PersistEvent(pEvent), cancellationToken);
                    }
                },cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not remove FTP Account for {partnerId}");
                return PersistEvent(pEvent, e);
            }
        }

        public Task CreateDataProcessingDirectoriesCommand(string partnerId, string partnerName, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating data processing directories.");
            var pEvent = new ProvisioningActionEvent
            {
                PartnerId = partnerId,
                Type = ProvisioningActionType.DataProcessingDirectoriesProvisioned,
                Started = DateTime.UtcNow
            };

            try
            {
                //TODO: this should be moved out of here and done once per environment
                //var createFileSystemTasks = DATA_CONTAINERS 
                //    .Select(f => _dataPipelineStorage.CreateFileSystem(f, cancellationToken))
                //    .ToArray();
                //Task.WhenAll(createFileSystemTasks).Wait(cancellationToken);

                var createDirectoryTasks = DATA_RESOURCES
                    .Select(dr => Task.WhenAll(
                        _dataPipelineStorage.CreateDirectory(dr.Key, partnerId, cancellationToken),
                        _tableStorageService.InsertAsync(new ProvisionedResourceEvent
                        {
                            PartnerId = partnerId,
                            Type = dr.Value,
                            ParentLocation = dr.Key,
                            Location = partnerId,
                            DisplayName = $"{partnerName} {dr.Key} Directory",
                            ProvisionedOn = DateTime.UtcNow
                        })
                ));

                return Task.WhenAll(createDirectoryTasks).ContinueWith(t => PersistEvent(pEvent), cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not complete provisioning on Data Processing Directories for {partnerId}");
                return PersistEvent(pEvent, e);
            }
        }

        private Task DeleteDataProcessingDirectories(string partnerId, CancellationToken cancellationToken)
        {
            var pEvent = new ProvisioningActionEvent
            {
                PartnerId = partnerId,
                Type = ProvisioningActionType.DataProcessingDirectoriesRemoved,
                Started = DateTime.UtcNow
            };
            try
            {
                var deleteDirectoryTasks = DATA_RESOURCES
                    .Select(f => Task.WhenAll( 
                        _dataPipelineStorage.CreateDirectory(f.Key, partnerId, cancellationToken),
                        _tableStorageService.DeleteAsync<ProvisionedResourceEvent>(partnerId, $"{f.Key}-{partnerId}")));
                return Task.WhenAll(deleteDirectoryTasks).ContinueWith(t => PersistEvent(pEvent), cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not remove Data Processing Directories for {partnerId}");
                return PersistEvent(pEvent, e);
            }
        }

        private Task PersistEvent(ProvisioningActionEvent pEvent, Exception e = null)
        {
            pEvent.Completed = DateTime.UtcNow;
            pEvent.ErrorMessage = e?.Message ?? string.Empty;
            return _tableStorageService.InsertOrReplaceAsync(pEvent);
        }

        private static string GetEscrowContainerName(string partnerId)
        {
            return $"transfer-{partnerId}";
        }

        // TODO: Move this into class, injected into "commands". [jay_mclain]
        private static string GetRandomAlphanumericString(int length)
        {
            // NEVER ALLOW ':' character - it will break sFTP account creation
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
