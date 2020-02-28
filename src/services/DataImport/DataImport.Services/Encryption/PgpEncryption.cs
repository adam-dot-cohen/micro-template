using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Laso.DataImport.Core.Common;
using Laso.DataImport.Core.Configuration;
using Laso.DataImport.Core.IO;
using Laso.DataImport.Services.DTOs;
using Laso.DataImport.Services.Security;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

// todo: instead of using secure store, use config with the KeyVault provider instead
namespace Laso.DataImport.Services.Encryption
{
    public class PgpEncryption : IPgpEncryption
    {
        private const string Identity = "devops <devops@quarterspot.com>";
        private static readonly SecureRandom Random = new SecureRandom();
        private readonly ISecureStore _secureStore;
        private readonly IEncryptionConfiguration _config;

        public PgpEncryption(ISecureStore secureStore, IEncryptionConfiguration config)
        {
            _secureStore = secureStore;
            _config = config;
        }

        public EncryptionType Type => EncryptionType.Pgp;
        public string FileExtension => ".gpg";

        public string GenerateKey(string passPhraseVaultName)
        {
            passPhraseVaultName ??= _config.QuarterSpotPgpPrivateKeyPassPhraseVaultName;

            var keyPairGenerator = new RsaKeyPairGenerator();
            var keyGenerationParams = new RsaKeyGenerationParameters(BigInteger.ValueOf(65537), Random, 2048, 5);
            keyPairGenerator.Init(keyGenerationParams);

            var keyPair = keyPairGenerator.GenerateKeyPair();

            var secretKey = new PgpSecretKey(
                PgpSignature.DefaultCertification,
                PublicKeyAlgorithmTag.RsaGeneral,
                keyPair.Public,
                keyPair.Private,
                DateTime.Now,
                Identity,
                SymmetricKeyAlgorithmTag.Aes256,
                passPhraseVaultName.ToCharArray(),
                null,
                null,
                new SecureRandom());

            var publicKey = GetKeyBlock(secretKey.PublicKey.Encode);
            var privateKey = GetKeyBlock(secretKey.Encode);

            return publicKey + privateKey;
        }

        private static string GetKeyBlock(Action<Stream> encode)
        {
            using var keyStream = new MemoryStream();
            using var armoredKeyStream = new ArmoredOutputStream(keyStream);
                
            encode(armoredKeyStream);

            keyStream.Seek(0, SeekOrigin.Begin);

            using var keyStreamReader = new StreamReader(keyStream);

            return keyStreamReader.ReadToEnd();
        }

        Task IEncryption.Encrypt(StreamStack streamStack)
        {
            return Encrypt(streamStack);
        }

        public async Task Encrypt(StreamStack streamStack, string publicKeyVaultName = null)
        {
            publicKeyVaultName ??= _config.QuarterSpotPgpPublicKeyVaultName;
            var encryptedDataGenerator = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Aes256, true, Random);
            encryptedDataGenerator.AddMethod(await GetPublicKey(publicKeyVaultName));

            var encryptedOutputStream = encryptedDataGenerator.Open(streamStack.Stream, new byte[65536]);
            var compressedDataGenerator = new PgpCompressedDataGenerator(CompressionAlgorithmTag.ZLib);
            var compressedStream = compressedDataGenerator.Open(encryptedOutputStream);
            var literalDataGenerator = new PgpLiteralDataGenerator();
            var literalStream = literalDataGenerator.Open(compressedStream, PgpLiteralData.Binary, PgpLiteralData.Console, DateTime.UtcNow, new byte[65536]);

            streamStack.Push(
                new DisposeAction(encryptedDataGenerator.Close),
                encryptedOutputStream,
                new DisposeAction(compressedDataGenerator.Close),
                compressedStream,
                new DisposeAction(literalDataGenerator.Close),
                literalStream);
        }

        private async Task<PgpPublicKey> GetPublicKey(string publicKeyVaultName)
        {
            var publicKey = await _secureStore.GetSecretAsync(publicKeyVaultName).ConfigureAwait(false);

            await using var publicKeyStream = new MemoryStream(Encoding.UTF8.GetBytes(publicKey));
            await using var decoderStream = PgpUtilities.GetDecoderStream(publicKeyStream);

            return new PgpPublicKeyRingBundle(decoderStream).GetKeyRings()
                .Cast<PgpPublicKeyRing>()
                .Select(r => r.GetPublicKeys()
                    .Cast<PgpPublicKey>()
                    .FirstOrDefault(k => k.IsEncryptionKey))
                .FirstOrDefault();
        }

        Task IEncryption.Decrypt(StreamStack stream)
        {
            return Decrypt(stream);
        }

        public async Task Decrypt(StreamStack stream, string privateKeyVaultName = null, string passPhraseKeyVaultName = null)
        {
            privateKeyVaultName ??= _config.QuarterSpotPgpPrivateKeyVaultName;
            passPhraseKeyVaultName ??= _config.QuarterSpotPgpPrivateKeyPassPhraseVaultName;

            var privateKey = await GetPrivateKey(privateKeyVaultName, passPhraseKeyVaultName);

            var decoderStream = PgpUtilities.GetDecoderStream(stream.Stream);

            stream.Push(decoderStream);

            var pgpObjectFactory = new PgpObjectFactory(decoderStream);

            var encryptedDataList = pgpObjectFactory.NextPgpObject() is PgpEncryptedDataList list
                ? list
                : (PgpEncryptedDataList)pgpObjectFactory.NextPgpObject();

            var encryptedDataObject = encryptedDataList.GetEncryptedDataObjects()
                .Cast<PgpPublicKeyEncryptedData>()
                .First();

            var encryptedStream = encryptedDataObject.GetDataStream(privateKey);

            stream.Push(encryptedStream);

            var data = new PgpObjectFactory(encryptedStream).NextPgpObject();

            if (data is PgpCompressedData compressedData)
            {
                var compressedDataStream = compressedData.GetDataStream();

                data = new PgpObjectFactory(compressedDataStream).NextPgpObject();

                stream.Push(compressedDataStream);
            }

            if (!(data is PgpLiteralData literalData))
                throw new NotSupportedException();

            var literalDataStream = literalData.GetInputStream();

            stream.Push(literalDataStream);
        }

        private async Task<PgpPrivateKey> GetPrivateKey(string privateKeyVaultName, string passPhraseVaultName)
        {
            var privateKey = await _secureStore.GetSecretAsync(privateKeyVaultName);

            using var privateKeyStream = new MemoryStream(Encoding.UTF8.GetBytes(privateKey));
            using var decoderStream = PgpUtilities.GetDecoderStream(privateKeyStream);

            var secretKey = new PgpSecretKeyRingBundle(decoderStream).GetKeyRings()
                .Cast<PgpSecretKeyRing>()
                .Select(r => r.GetSecretKeys()
                    .Cast<PgpSecretKey>()
                    .FirstOrDefault())
                .First();

            var passPhrase = await _secureStore.GetSecretAsync(passPhraseVaultName);

            return secretKey.ExtractPrivateKey(passPhrase.ToCharArray());
        }
    }
}
