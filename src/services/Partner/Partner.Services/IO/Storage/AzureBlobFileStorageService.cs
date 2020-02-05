using System;
using System.IO;

namespace Partner.Services.IO.Storage
{
    public class AzureReadOnlyBlobStorageService : IReadOnlyBlobStorageService
    {
        public bool Exists(string container, string blobName)
        {
            throw new NotImplementedException();
        }

        public Stream OpenRead(string container, string blobName)
        {
            throw new NotImplementedException();
        }
    }

    public class AzureBlobStorageService : AzureReadOnlyBlobStorageService, IBlobStorageService
    {
        public void Delete(string container, string name)
        {
            throw new NotImplementedException();
        }

        public Stream OpenWrite(string containerName, string blobName, string fileName = null, long? length = null)
        {
            throw new NotImplementedException();
        }
    }
}
