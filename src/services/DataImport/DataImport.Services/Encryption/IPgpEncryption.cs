using System.Threading.Tasks;
using Laso.DataImport.Core.IO;
using Laso.DataImport.Services.Security;

namespace Laso.DataImport.Services.Encryption
{
    public interface IPgpEncryption : IEncryption
    {
        string GenerateKey(string passPhraseVaultName = null);
        Task Encrypt(StreamStack stream, string publicKeyVaultName = null);
        Task Decrypt(StreamStack stream, string privateKeyVaultName = null, string passPhraseVaultName = null);
    }
}
