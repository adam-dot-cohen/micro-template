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
    public class AzureBlobStorageService : IBlobStorageService, IColdBlobStorageService, IEscrowBlobStorageService
    {
        private const string AnchorFileName = ".anchor";

        private readonly BlobServiceClient _client;

        public AzureBlobStorageService(BlobServiceClient client)
        {
            _client = client;
        }

        public Task CreateContainer(string name, CancellationToken cancellationToken)
        {
            return _client.CreateBlobContainerAsync(name, cancellationToken: cancellationToken);
        }

        public async Task CreateContainerIfNotExists(string name, CancellationToken cancellationToken)
        {
            try
            {
                await _client.CreateBlobContainerAsync(name, cancellationToken: cancellationToken);
            }
            catch (RequestFailedException e)
                when (e.ErrorCode == BlobErrorCode.ContainerAlreadyExists)
            {
                // Exists
            }
        }

        public Task DeleteContainer(string name, CancellationToken cancellationToken)
        {
            return _client.DeleteBlobContainerAsync(name, cancellationToken: cancellationToken);
        }

        public async Task DeleteContainerIfExists(string name, CancellationToken cancellationToken)
        {
            try
            {
                await _client.DeleteBlobContainerAsync(name, cancellationToken: cancellationToken);
            }
            catch (RequestFailedException e)
                when (e.ErrorCode == BlobErrorCode.ContainerNotFound)
            {
                // Doesn't Exist
            }
        }

        public Task CreateDirectory(string containerName, string path, CancellationToken cancellationToken)
        {
            return UploadTextBlob(containerName, $"{path}/{AnchorFileName}", string.Empty, CancellationToken.None);
        }

        public Task CreateDirectoryIfNotExists(string containerName, string path, CancellationToken cancellationToken)
        {
            return ReplaceTextBlob(containerName, $"{path}/{AnchorFileName}", string.Empty, CancellationToken.None);
        }

        public async Task UploadTextBlob(string containerName, string path, string text, CancellationToken cancellationToken)
        {
            var container = _client.GetBlobContainerClient(containerName);

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                await container.UploadBlobAsync(path, stream, cancellationToken);
            }
        }

        public async Task ReplaceTextBlob(string containerName, string path, string text, CancellationToken cancellationToken)
        {
            var container = _client.GetBlobContainerClient(containerName);

            await container.DeleteBlobIfExistsAsync(path, cancellationToken: cancellationToken);

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                await container.UploadBlobAsync(path, stream, cancellationToken);
            }
        }
    }
}
