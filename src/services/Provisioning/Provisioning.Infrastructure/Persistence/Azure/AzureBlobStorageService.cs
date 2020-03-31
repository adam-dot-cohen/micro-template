using System;
using System.Linq;
using Laso.Provisioning.Core.Persistence;
using Microsoft.Azure.Storage.Blob;

namespace Provisioning.Infrastructure.Persistence.Azure
{
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly Lazy<CloudBlobClient> _client;
        private const string AnchorFileName = ".anchor";

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

        public void CreateDirectory(string containerName, string path)
        {
            var container = _client.Value.GetContainerReference(containerName.ToLower());

            if (!container.Exists()) return;

            var blobDirectory = container.GetDirectoryReference(path.ToLower());

            if(blobDirectory?.ListBlobs().FirstOrDefault() != null) return;

            var anchorBlob = $"{path}/{AnchorFileName}";
            var blob = container.GetBlockBlobReference(anchorBlob);
            blob.UploadText("");
        }

        public void WriteTextToFile(string containerName, string path, string text)
        {
            var container = _client.Value.GetContainerReference(containerName.ToLower());

            if(!container.Exists()) return;

            var blob = container.GetBlockBlobReference(path.ToLower());
            blob.UploadText(text);
        }
    }
}
