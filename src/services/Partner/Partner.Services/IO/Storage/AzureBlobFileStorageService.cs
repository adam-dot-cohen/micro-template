using System;
using System.IO;

namespace Partner.Services.IO.Storage
{
    public class AzureBlobFileStorageService : IFileStorageService
    {
        public void Delete(StorageMoniker moniker)
        {
            throw new NotImplementedException();
        }

        public bool Exists(StorageMoniker moniker)
        {
            throw new NotImplementedException();
        }

        public Stream OpenRead(StorageMoniker moniker)
        {
            throw new NotImplementedException();
        }

        public Stream OpenWrite(StorageMoniker moniker)
        {
            throw new NotImplementedException();
        }
    }
}
