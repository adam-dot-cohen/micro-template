using System.Linq;
using Laso.Provisioning.Core.Persistence;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;

namespace Provisioning.Infrastructure.Persistence.Azure
{
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly CloudBlobClient _client;
        private const string AnchorFileName = ".anchor";

        public AzureBlobStorageService(IConfiguration configuration)
        {
            // TODO: Eventually, move this to dependency resolution. [jay_mclain]
            // NOTE: Using "Identity" storage connection string for now...need to resolve
            // service configuration URIs. [jay_mclain]
            var connectionString = configuration.GetConnectionString("EscrowStorage");
            _client = CloudStorageAccount.Parse(connectionString).CreateCloudBlobClient();
        }

        public void CreateContainer(string name)
        {
            var container = _client.GetContainerReference(name.ToLower());

            if (!container.Exists())
                container.CreateIfNotExists();
        }

        public void CreateDirectory(string containerName, string path)
        {
            var container = _client.GetContainerReference(containerName.ToLower());

            if (!container.Exists()) return;

            var blobDirectory = container.GetDirectoryReference(path.ToLower());

            if(blobDirectory?.ListBlobs().FirstOrDefault() != null) return;

            var anchorBlob = $"{path}/{AnchorFileName}";
            var blob = container.GetBlockBlobReference(anchorBlob);
            blob.UploadText("");
        }

        public void WriteTextToFile(string containerName, string path, string text)
        {
            var container = _client.GetContainerReference(containerName.ToLower());

            if(!container.Exists()) return;

            var blob = container.GetBlockBlobReference(path.ToLower());
            blob.UploadText(text);
        }
    }
}
