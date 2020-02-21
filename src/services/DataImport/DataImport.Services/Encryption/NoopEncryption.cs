using System.Threading.Tasks;
using Laso.DataImport.Core.IO;
using Laso.DataImport.Services.DTOs;

namespace Laso.DataImport.Services.Encryption
{
    public class NoopEncryption : IEncryption
    {
        public EncryptionType Type => EncryptionType.None;
        public string FileExtension => "";

        public Task Encrypt(StreamStack stream)
        {
            return Task.FromResult(stream);
        }

        public Task Decrypt(StreamStack stream)
        {
            return Task.FromResult(stream);
        }
    }
}
