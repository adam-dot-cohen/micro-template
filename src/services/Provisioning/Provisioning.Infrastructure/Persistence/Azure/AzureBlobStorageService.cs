using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Laso.Provisioning.Core.Persistence;

namespace Laso.Provisioning.Infrastructure.Persistence.Azure
{
    public class AzureBlobStorageService : IBlobStorageService
    {
        private const string AnchorFileName = ".anchor";

        private readonly BlobServiceClient _client;

        public AzureBlobStorageService(BlobServiceClient client)
        {
            _client = client;
        }

        public async Task CreateContainer(string name, CancellationToken cancellationToken)
        {
            try
            {
                await _client.CreateBlobContainerAsync(name, cancellationToken: cancellationToken);
            }
            catch (RequestFailedException storageRequestFailedException)
                when (storageRequestFailedException.ErrorCode == BlobErrorCode.ContainerAlreadyExists)
            {
                // TODO: Optional?
            }
        }

        public Task DeleteContainer(string name, CancellationToken cancellationToken)
        {
            return _client.DeleteBlobContainerAsync(name, cancellationToken: cancellationToken);
        }

        public Task CreateDirectory(string containerName, string path, CancellationToken cancellationToken)
        {
            return UploadTextBlob(containerName, $"{path}/{AnchorFileName}", string.Empty, CancellationToken.None);
        }

        public async Task UploadTextBlob(string containerName, string path, string text, CancellationToken cancellationToken)
        {
            var container = _client.GetBlobContainerClient(containerName);

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                try
                {
                    await container.UploadBlobAsync(path, stream, cancellationToken);
                }
                catch (RequestFailedException storageRequestFailedException)
                    when (storageRequestFailedException.ErrorCode == BlobErrorCode.UnauthorizedBlobOverwrite)
                {
                    // TODO: Optional?
                }
            }
        }
    }
}
