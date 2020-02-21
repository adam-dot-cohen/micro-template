using System.Linq;
using System.Collections.Generic;
using Laso.DataImport.Services.DTOs;

namespace Laso.DataImport.Services.Encryption
{
    public interface IEncryptionFactory
    {
        IEncryption Create(EncryptionType type);
    }

    public class EncryptionFactory : IEncryptionFactory
    {
        private readonly IEnumerable<IEncryption> _encrypters;

        public EncryptionFactory(IEnumerable<IEncryption> encrypters)
        {
            _encrypters = encrypters;
        }

        public IEncryption Create(EncryptionType type)
        {
            return _encrypters.FirstOrDefault(e => e.Type == type);
        }
    }
}
