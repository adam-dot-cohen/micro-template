using System;
using Laso.Provisioning.Core.Persistence;
using Microsoft.Azure.Storage.Blob;

namespace Provisioning.Infrastructure.Persistence.Azure
{
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly Lazy<CloudBlobClient> _client;

        public AzureBlobStorageService(Lazy<CloudBlobClient> client)
        {
            _client = client;
        }

        public void CreateContainer(string name)
        {
            var container = _client.Value.GetContainerReference(name.ToLower());

            if (!container.Exists())
                container.CreateIfNotExists();
        }
    }
}
