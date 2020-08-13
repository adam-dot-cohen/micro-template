using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Laso.IntegrationEvents;
using Laso.IntegrationMessages;
using Laso.Provisioning.Core;
using Laso.Provisioning.Core.Messaging.Encryption;
using Laso.Provisioning.Infrastructure.Utilities;
using Laso.TableStorage;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Provisioning.Domain.Entities;
using Exception = System.Exception;

namespace Laso.Provisioning.Infrastructure.Encryption
{
    public class CreatePgpKeySetHandler : ICommandHandler<CreatePgpKeySetCommand>
    {
        private readonly ILogger<CreatePgpKeySetHandler> _logger;
        private readonly IEventPublisher _bus;
        private readonly IApplicationSecrets _secrets;
        private readonly ITableStorageService _tableStorage;

        public CreatePgpKeySetHandler(ILogger<CreatePgpKeySetHandler> logger, IEventPublisher bus, IApplicationSecrets secrets, ITableStorageService tableStorage)
        {
            _logger = logger;
            _bus = bus;
            _secrets = secrets;
            _tableStorage = tableStorage;
        }

        public Task Handle(CreatePgpKeySetCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating partner PGP key set.");

            var pubKeyName = string.Format(ResourceLocations.LASO_PGP_PUBLIC_KEY_FORMAT, command.PartnerId);
            var privKeyName = string.Format(ResourceLocations.LASO_PGP_PRIVATE_KEY_FORMAT, command.PartnerId);
            var passName = string.Format(ResourceLocations.LASO_PGP_PASSPHRASE_FORMAT, command.PartnerId);
            var passPhrase = StringUtilities.GetRandomAlphanumericString(10);
            var (publicKey, privateKey) = GenerateKeySet("Laso Insights <insights@laso.com>", passPhrase);
            //add logic to check the key and publish failure if appropriate
            try
            {
                var pubKey = _secrets.SetSecret(pubKeyName, publicKey, cancellationToken);
                pubKey.Wait(cancellationToken);
                var recPubKey = _tableStorage.InsertAsync(new ProvisionedResourceEvent
                {
                    PartnerId = command.PartnerId,
                    Type = ProvisionedResourceType.LasoPGPPublicKey,
                    ParentLocation = ResourceLocations.GetParentLocationByType(ProvisionedResourceType.LasoPGPPublicKey),
                    Location = pubKeyName,
                    DisplayName = "Laso Public Key",
                    ProvisionedOn = DateTime.UtcNow,
                    Sensitive = true
                });
                recPubKey.Wait(cancellationToken);

                var privKey = _secrets.SetSecret(privKeyName, privateKey, cancellationToken);
                privKey.Wait(cancellationToken);
                var recPriv = _tableStorage.InsertAsync(new ProvisionedResourceEvent
                {
                    PartnerId = command.PartnerId,
                    Type = ProvisionedResourceType.LasoPGPPrivateKey,
                    ParentLocation = ResourceLocations.GetParentLocationByType(ProvisionedResourceType.LasoPGPPrivateKey),
                    Location = privKeyName,
                    DisplayName = "Laso Private Key",
                    ProvisionedOn = DateTime.UtcNow,
                    Sensitive = true
                });
                recPriv.Wait(cancellationToken);
                var pass = _secrets.SetSecret(passName, passPhrase, cancellationToken);
                pass.Wait(cancellationToken);
                var recPass = _tableStorage.InsertAsync(new ProvisionedResourceEvent
                {
                    PartnerId = command.PartnerId,
                    Type = ProvisionedResourceType.LasoPGPPassphrase,
                    ParentLocation =
                        ResourceLocations.GetParentLocationByType(ProvisionedResourceType.LasoPGPPassphrase),
                    Location = passName,
                    DisplayName = "Laso PGP Pass Phrase",
                    ProvisionedOn = DateTime.UtcNow,
                    Sensitive = true
                });
                recPass.Wait(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not create at least one element of the pgp key set for {command.PartnerId}.  Resolve the issues and re-issue the command.");
                throw; //we want the message to dead letter
            }

            return _bus.Publish(new PartnerPgpKeySetCreatedEvent
                {Completed= DateTime.UtcNow, PartnerId = command.PartnerId});
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