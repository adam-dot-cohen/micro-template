using System.Threading.Tasks;
using Laso.DataImport.Core.IO;
using Laso.DataImport.Services.DTOs;

namespace Laso.DataImport.Services.Encryption
{
    public interface IEncryption
    {
        EncryptionType Type { get; }
        Task Encrypt(StreamStack stream);
        Task Decrypt(StreamStack stream);
    }
}
