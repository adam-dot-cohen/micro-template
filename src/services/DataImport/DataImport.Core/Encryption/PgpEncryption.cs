using System;
using System.IO;
using System.Linq;
using Laso.DataImport.Core.Common;
using Laso.DataImport.Core.IO;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Laso.DataImport.Core.Encryption
{
    public class PgpEncryption : IPgpEncryption
    {
        private const string Identity = "devops <devops@quarterspot.com>";
        private static readonly SecureRandom Random = new SecureRandom();

        public string GenerateKey(string passPhrase)
        {
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
                passPhrase.ToCharArray(),
                null,
                null,
                new SecureRandom());

            var publicKey = GetKeyBlock(secretKey.PublicKey.Encode);
            var privateKey = GetKeyBlock(secretKey.Encode);

            return publicKey + privateKey;
        }

        private static string GetKeyBlock(Action<Stream> encode)
        {
            using (var keyStream = new MemoryStream())
            {
                using (var armoredKeyStream = new ArmoredOutputStream(keyStream))
                {
                    encode(armoredKeyStream);
                }

                keyStream.Seek(0, SeekOrigin.Begin);

                using (var keyStreamReader = new StreamReader(keyStream))
                {
                    return keyStreamReader.ReadToEnd();
                }
            }
        }
        
        public void Encrypt(StreamStack streamStack, byte[] publicKey)
        {
            var encryptedDataGenerator = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Aes256, true, Random);
            encryptedDataGenerator.AddMethod(GetPublicKey(publicKey));

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

        private PgpPublicKey GetPublicKey(byte[] publicKey)
        {
            using var publicKeyStream = new MemoryStream(publicKey);
            using var decoderStream = PgpUtilities.GetDecoderStream(publicKeyStream);

            return new PgpPublicKeyRingBundle(decoderStream).GetKeyRings()
                .Cast<PgpPublicKeyRing>()
                .Select(r => r.GetPublicKeys()
                    .Cast<PgpPublicKey>()
                    .FirstOrDefault(k => k.IsEncryptionKey))
                .FirstOrDefault();
        }

        public void Decrypt(StreamStack streamStack, byte[] privateKey, string passPhrase)
        {
            var pgpKey = GetPrivateKey(privateKey, passPhrase);

            var decoderStream = PgpUtilities.GetDecoderStream(streamStack.Stream);

            streamStack.Push(decoderStream);

            var pgpObjectFactory = new PgpObjectFactory(decoderStream);

            var encryptedDataList = pgpObjectFactory.NextPgpObject() is PgpEncryptedDataList list
                ? list
                : (PgpEncryptedDataList)pgpObjectFactory.NextPgpObject();

            var encryptedDataObject = encryptedDataList.GetEncryptedDataObjects()
                .Cast<PgpPublicKeyEncryptedData>()
                .First();

            var encryptedStream = encryptedDataObject.GetDataStream(pgpKey);

            streamStack.Push(encryptedStream);

            var data = new PgpObjectFactory(encryptedStream).NextPgpObject();

            if (data is PgpCompressedData compressedData)
            {
                var compressedDataStream = compressedData.GetDataStream();

                data = new PgpObjectFactory(compressedDataStream).NextPgpObject();

                streamStack.Push(compressedDataStream);
            }

            if (!(data is PgpLiteralData literalData))
                throw new NotSupportedException();

            var literalDataStream = literalData.GetInputStream();

            streamStack.Push(literalDataStream);
        }

        private PgpPrivateKey GetPrivateKey(byte[] privateKeyBytes, string passPhrase)
        {
            PgpSecretKey secretKey;
            using var privateKeyStream = new MemoryStream(privateKeyBytes);
            using var decoderStream = PgpUtilities.GetDecoderStream(privateKeyStream);

            secretKey = new PgpSecretKeyRingBundle(decoderStream).GetKeyRings()
                .Cast<PgpSecretKeyRing>()
                .Select(r => r.GetSecretKeys()
                    .Cast<PgpSecretKey>()
                    .FirstOrDefault())
                .First();

            return secretKey.ExtractPrivateKey(passPhrase.ToCharArray());
        }
    }
}
