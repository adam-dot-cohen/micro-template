using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Laso.Provisioning.Core;
using Microsoft.Extensions.Configuration;

namespace Laso.Provisioning.Infrastructure
{
    public class AzureDataLakeDataPipelineStorage : IDataPipelineStorage
    {
        private readonly DataLakeServiceClient _client;

        public AzureDataLakeDataPipelineStorage(IConfiguration configuration)
        {
            var serviceUri = new Uri(configuration["AzureDataLake:BaseUrl"]);
            var credential = new StorageSharedKeyCredential(
                configuration["AzureDataLake:AccountName"], configuration["AzureDataLake:AccountKey"]);

            // TODO: Abstraction for DataLakeServiceClient
            _client = new DataLakeServiceClient(serviceUri, credential);
        }

        public async Task CreateFileSystem(string fileSystemName, CancellationToken cancellationToken)
        {
            var fileSystemClient = _client.GetFileSystemClient(fileSystemName);
            bool exists = await fileSystemClient.ExistsAsync(cancellationToken);

            if (!exists)
                await fileSystemClient.CreateAsync(cancellationToken: cancellationToken);
        }

        public Task CreateDirectory(string fileSystemName, string directoryName, CancellationToken cancellationToken)
        {
            var fileSystem = _client.GetFileSystemClient(fileSystemName);
            return fileSystem.CreateDirectoryAsync(directoryName, cancellationToken: cancellationToken);
        }

        public Task DeleteFileSystem(string fileSystemName, CancellationToken cancellationToken)
        {
            return _client.DeleteFileSystemAsync(fileSystemName, cancellationToken: cancellationToken);
        }
    }
}
