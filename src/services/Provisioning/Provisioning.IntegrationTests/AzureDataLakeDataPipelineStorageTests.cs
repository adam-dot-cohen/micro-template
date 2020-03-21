using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.Provisioning.Infrastructure;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

namespace Laso.Provisioning.IntegrationTests
{
    public class AzureDataLakeDataPipelineStorageTests
    {
        [Fact]
        public async Task Should_Create_FileSystem()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var client = new AzureDataLakeDataPipelineStorage(configuration);

            var fileSystemName = Guid.NewGuid().ToString();
            var createdFileSystem = false;
            try
            {
                await client.CreateFileSystem(fileSystemName, CancellationToken.None);
                createdFileSystem = true;
            }
            finally
            {
                if (createdFileSystem)
                    await client.DeleteFileSystem(fileSystemName, CancellationToken.None);
            }
        }

        [Fact]
        public async Task Should_Create_Directory()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var client = new AzureDataLakeDataPipelineStorage(configuration);

            var fileSystemName = Guid.NewGuid().ToString();
            var createdFileSystem = false;
            var directoryName = Guid.NewGuid().ToString();
            var createdDirectory = false;

            try
            {
                await client.CreateFileSystem(fileSystemName, CancellationToken.None);
                createdFileSystem = true;

                await client.CreateDirectory(fileSystemName, directoryName, CancellationToken.None);
                createdDirectory = true;
            }
            finally
            {
                if (createdFileSystem)
                    await client.DeleteFileSystem(fileSystemName, CancellationToken.None);
            }

            createdFileSystem.ShouldBeTrue();
            createdDirectory.ShouldBeTrue();
        }
    }
}
