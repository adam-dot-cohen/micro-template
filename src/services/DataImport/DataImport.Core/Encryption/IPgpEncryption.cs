using Laso.DataImport.Core.IO;

namespace Laso.DataImport.Core.Encryption
{
    public interface IPgpEncryption
    {
        string GenerateKey(string passPhrase);
        void Encrypt(StreamStack streamStack, byte[] publicKey);
        void Decrypt(StreamStack streamStack, byte[] privateKey, string passPhrase);
    }
}
