using System.Threading.Tasks;
using Laso.DataImport.Core.IO;
using Laso.DataImport.Domain.Entities;

namespace Laso.DataImport.Services.Encryption
{
    public interface IEncryption
    {
        EncryptionType Type { get; }
        string FileExtension { get; }
        Task Encrypt(StreamStack stream);
        Task Decrypt(StreamStack stream);
    }
}
