using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Laso.Provisioning.Core;

namespace Laso.Provisioning.Infrastructure
{
    public class AzureBlobDataPipelineStorage : IDataPipelineStorage
    {
        private const string AnchorFileName = ".anchor";
        
        private readonly BlobServiceClient _client;

        public AzureBlobDataPipelineStorage(BlobServiceClient client)
        {
            _client = client;
        }

        public async Task CreateFileSystem(string fileSystemName, CancellationToken cancellationToken)
        {
            try
            {
                await _client.CreateBlobContainerAsync(fileSystemName, cancellationToken: cancellationToken);
            }
            catch (RequestFailedException storageRequestFailedException)
                when (storageRequestFailedException.ErrorCode == BlobErrorCode.ContainerAlreadyExists)
            {
                // TODO: Optional?
            }
        }

        public async Task CreateDirectory(string fileSystemName, string directoryName, CancellationToken cancellationToken)
        {
            try
            {
                await UploadTextBlob(fileSystemName, $"{directoryName}/{AnchorFileName}", string.Empty, CancellationToken.None);
            }
            catch (RequestFailedException storageRequestFailedException)
                when (storageRequestFailedException.ErrorCode == BlobErrorCode.UnauthorizedBlobOverwrite)
            {
                // TODO: Optional?
            }
        }

        public async Task DeleteDirectory(string fileSystemName, string directoryName, CancellationToken cancellationToken)
        {
            var container = _client.GetBlobContainerClient(fileSystemName);
            var exists = await container.ExistsAsync(cancellationToken);
            if (!exists)
                return;
            await container.DeleteBlobIfExistsAsync($"{directoryName}/{AnchorFileName}",
                DeleteSnapshotsOption.IncludeSnapshots, null, cancellationToken);
        }

        public Task UploadTextBlob(string containerName, string path, string text, CancellationToken cancellationToken)
        {
            var container = _client.GetBlobContainerClient(containerName);

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                return container.UploadBlobAsync(path, stream, cancellationToken);
            }
        }
    }
}
