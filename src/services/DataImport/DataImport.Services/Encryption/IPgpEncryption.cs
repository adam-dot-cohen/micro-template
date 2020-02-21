using System.Threading.Tasks;
using Laso.DataImport.Core.IO;
using Laso.DataImport.Services.Security;

namespace Laso.DataImport.Services.Encryption
{
    public interface IPgpEncryption : IEncryption
    {
        string GenerateKey(string passPhraseVaultName = KeyVaultNames.QuarterSpotPgpPrivateKeyPassPhrase);
        Task Encrypt(StreamStack stream, string publicKeyVaultName = KeyVaultNames.QuarterSpotPgpPublicKey);
        Task Decrypt(StreamStack stream, string privateKeyVaultName = KeyVaultNames.QuarterSpotPgpPrivateKey, string passPhraseVaultName = KeyVaultNames.QuarterSpotPgpPrivateKeyPassPhrase);
    }
}
