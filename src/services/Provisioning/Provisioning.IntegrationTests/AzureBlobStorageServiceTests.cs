using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;
using Laso.Provisioning.Infrastructure.Persistence.Azure;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

namespace Laso.Provisioning.IntegrationTests
{
    public class AzureBlobStorageServiceTests
    {
        [Fact]
        public async Task Should_Create_Container()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var serviceUrl = new Uri(configuration["Services:Provisioning:Partner.EscrowStorage:ServiceUrl"]);
            var client = new BlobServiceClient(serviceUrl, new DefaultAzureCredential());
            var blobStorageService = new AzureBlobStorageService(client);

            var containerName = $"{Guid.NewGuid():N}";
            var createdContainer = false;
            
            try
            {
                await blobStorageService.CreateContainer(containerName, CancellationToken.None);
                createdContainer = true;
            }
            finally
            {
                await blobStorageService.DeleteContainer(containerName, CancellationToken.None);
            }

            createdContainer.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_Create_Directory()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var serviceUrl = new Uri(configuration["Services:Provisioning:Partner.EscrowStorage:ServiceUrl"]);
            var client = new BlobServiceClient(serviceUrl, new DefaultAzureCredential());
            var blobStorageService = new AzureBlobStorageService(client);

            var containerName = $"{Guid.NewGuid():N}";
            var createdContainer = false;
            var directoryName = $"{Guid.NewGuid():N}/{Guid.NewGuid():N}";
            var createdDirectory = false;
            
            try
            {
                await blobStorageService.CreateContainer(containerName, CancellationToken.None);
                createdContainer = true;

                await blobStorageService.CreateDirectory(containerName, directoryName, CancellationToken.None);
                createdDirectory = true;
            }
            finally
            {
                await blobStorageService.DeleteContainer(containerName, CancellationToken.None);
            }

            createdContainer.ShouldBeTrue();
            createdDirectory.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_Recreate_Directory()
        {

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var serviceUrl = new Uri(configuration["Services:Provisioning:Partner.EscrowStorage:ServiceUrl"]);
            var client = new BlobServiceClient(serviceUrl, new DefaultAzureCredential());
            var blobStorageService = new AzureBlobStorageService(client);

            var containerName = $"{Guid.NewGuid():N}";
            var createdContainer = false;
            var directoryName = $"{Guid.NewGuid():N}/{Guid.NewGuid():N}";
            var createdDirectory = false;
            
            try
            {
                await blobStorageService.CreateContainer(containerName, CancellationToken.None);
                createdContainer = true;

                await blobStorageService.CreateDirectory(containerName, directoryName, CancellationToken.None);
                createdDirectory = true;

                await blobStorageService.CreateDirectory(containerName, directoryName, CancellationToken.None);
                createdDirectory = true;
            }
            finally
            {
                await blobStorageService.DeleteContainer(containerName, CancellationToken.None);
            }

            createdContainer.ShouldBeTrue();
            createdDirectory.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_UploadTextBlob_To_Path_In_Container()
        {

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var serviceUrl = new Uri(configuration["Services:Provisioning:Partner.EscrowStorage:ServiceUrl"]);
            var client = new BlobServiceClient(serviceUrl, new DefaultAzureCredential());
            var blobStorageService = new AzureBlobStorageService(client);

            var containerName = $"{Guid.NewGuid():N}";
            var createdContainer = false;
            var blobName = $"{containerName}/{Guid.NewGuid():N}/test.txt";
            var uploadedFile = false;
            var blobText = "some text";


            try
            {
                await blobStorageService.CreateContainer(containerName, CancellationToken.None);
                createdContainer = true;

                await blobStorageService.UploadTextBlob(containerName, blobName, blobText, CancellationToken.None);
                uploadedFile = true;
            }
            finally
            {
                //if (createdDirectory)
                //    await blobStorageService.DeleteContainer(directoryName, CancellationToken.None);

                if (createdContainer)
                    await blobStorageService.DeleteContainer(containerName, CancellationToken.None);
            }

            createdContainer.ShouldBeTrue();
            uploadedFile.ShouldBeTrue();
        }
    }
}
