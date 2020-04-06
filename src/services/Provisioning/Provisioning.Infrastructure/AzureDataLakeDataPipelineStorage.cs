﻿using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Files.DataLake;
using Laso.Provisioning.Core;

namespace Laso.Provisioning.Infrastructure
{
    public class AzureDataLakeDataPipelineStorage : IDataPipelineStorage
    {
        private readonly DataLakeServiceClient _client;

        public AzureDataLakeDataPipelineStorage(DataLakeServiceClient client)
        {
            _client = client;
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
