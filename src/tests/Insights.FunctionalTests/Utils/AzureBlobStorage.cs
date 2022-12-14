using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Laso.Insights.FunctionalTests.Services.DataPipeline;
using Newtonsoft.Json;
using Uri = System.Uri;

namespace Laso.Insights.FunctionalTests.Utils
{
    class AzureBlobStgFactory
    {
      

        private class AzureBlobStg : IAzureBlobStg
        {
            public async Task<bool> FileExists(string fileName, StorageConfig config, string container)
            {
                var blobServiceClient = CloudBlobClient(config);
                var containerClient = blobServiceClient.GetBlobContainerClient(container);
                return await containerClient.GetBlobClient(fileName).ExistsAsync();
            }

            public List<BlobItem> GetFilesInBlob(StorageConfig config, string container, string directory)
            {
                var blobServiceClient = CloudBlobClient(config);
                var containerClient = blobServiceClient.GetBlobContainerClient(container);
                var dirBlobItems = containerClient.GetBlobs(prefix: directory,traits:BlobTraits.All);
                return dirBlobItems.ToList();
               
            }

            private static BlobServiceClient CloudBlobClient(StorageConfig config)
            {
                var tokenCredential = new EnvironmentCredential();
                Uri serviceUri = new Uri($"https://{config.Account}.blob.core.windows.net");
                return new BlobServiceClient(serviceUri, tokenCredential);
            }

            public async Task<Manifest> DownloadFile(StorageConfig config, string container, string fileName)

            {
                var serviceClient = CloudBlobClient(config);
                var containerCLient = serviceClient.GetBlobContainerClient(container);

                var blobClient = containerCLient.GetBlobClient(fileName);


                var fileExists = await blobClient.ExistsAsync();


                if (!fileExists)
                {
                    throw new Exception("File " + blobClient.Uri + " does not exist ");
                }

                string text;
                await using (var memoryStream = new MemoryStream())
                {
                    await blobClient.DownloadToAsync(memoryStream);
                    text = Encoding.UTF8.GetString(memoryStream.ToArray());
                }

                return
                    JsonConvert.DeserializeObject<Manifest>(text);
            }

            public async Task<string[]> DownloadCsvFile(string fileName, StorageConfig config, string container)
            {

                var client = CloudBlobClient(config);
                var containerClient = client.GetBlobContainerClient(container);
                var blobClient = containerClient.GetBlobClient(fileName);


                var fileExists = await blobClient.ExistsAsync();


                if (!fileExists)
                {
                    throw new Exception($"File {fileName} does not exist ");
                }

                string[] lines;
                // List<string> lines = new List<string>();
                string text;
                await using (var memoryStream = new MemoryStream())
                {
                    await blobClient.DownloadToAsync(memoryStream);
                    text = Encoding.UTF8.GetString(memoryStream.ToArray());
                    lines = text.Split(
                        new[] {Environment.NewLine, "\r", "\n"},
                        StringSplitOptions.None
                    );

                }

                return lines;

            }

            public async Task<string[]> DownloadCsvFileFromAutomationStorage(string fileName)

            {
                var client = CloudBlobClient(GlobalSetUp.TestConfiguration.AutomationStorage);
                return await DownloadFile(fileName, client);
            }


            public async Task<string[]> DownloadCsvFileFromMain(string fileName)

            {
                var client = CloudBlobClient(GlobalSetUp.TestConfiguration.MainInsightsStorage);
                return await DownloadFile(fileName, client);
            }


            public async Task<string[]> DownloadCsvFileFromCold(string fileName)

            {
                var client = CloudBlobClient(GlobalSetUp.TestConfiguration.ColdStorage);
                return await DownloadFile(fileName, client);
            }
            public async Task<string[]> DownloadCsvFileFromEscrow(string fileName)

            {
                var client = CloudBlobClient(GlobalSetUp.TestConfiguration.EscrowStorage);
                return await DownloadFile(fileName, client);
            }

            private static async Task<string[]> DownloadFile(string fileNameOrg, BlobServiceClient client)
            {
                var containerClient =
                    client.GetBlobContainerClient(GlobalSetUp.TestConfiguration.AutomationPartner.Container);

                var blockBlob = containerClient.GetBlobClient(fileNameOrg);

                var fileExists = await blockBlob.ExistsAsync();


                if (!fileExists)
                {
                    throw new Exception("File " + blockBlob.Uri + " does not exist ");
                }

                await using var memoryStream = new MemoryStream();
                await blockBlob.DownloadToAsync(memoryStream);
                var text = Encoding.UTF8.GetString(memoryStream.ToArray());
                var lines = text.Split(
                    new[] {Environment.NewLine, "\r", "\n"},
                    StringSplitOptions.None
                );

                return lines;
            }


            public async Task<IBlobInfo> CopyFile(BlobMeta source,BlobMeta dest)
            {


                var sourceClient = CloudBlobClient(source.Config);
                var containerClient = sourceClient.GetBlobContainerClient(source.ContainerName);

                var destClient = CloudBlobClient(dest.Config);
                var destContainer = destClient.GetBlobContainerClient(dest.ContainerName);
                var cloudBlockBlobDest = destContainer.GetBlobClient(dest.FileName);
                
                BlobClient blob=  containerClient.GetBlobClient(source.FileName);
                await cloudBlockBlobDest.UploadAsync(blob.DownloadAsync().Result.Value.Content);
              

                var destinationClient = destContainer.GetBlobClient(dest.FileName);

                var fileExists = await destinationClient.ExistsAsync();


                if (!fileExists)
                    throw new Exception("File " + dest.FileName + " was not copied successfully to " +
                        dest.Config.Account + " " + dest.ContainerName);
                return
                    new BlobInfo
                    {
                        AbsoluteUrl = destinationClient.Uri.AbsoluteUri,
                        FileName = destinationClient.Name,
                        Contents = null
                    };
            }
        }

        public IAzureBlobStg Create()
        {
            return new AzureBlobStg();
        }

    }


    public class BlobMeta
    {

        public StorageConfig Config { get; set; }
        public string ContainerName { get; set; }
        public string FileName{ get; set; }
    }


    public interface IAzureBlobStg
    {

        Task<string[]> DownloadCsvFile(string fileName, StorageConfig config, string container);
        List<BlobItem> GetFilesInBlob(StorageConfig config, string container, string directory);
        Task<Manifest> DownloadFile(StorageConfig config, string container, string fileName);

        Task<IBlobInfo> CopyFile(BlobMeta source,BlobMeta dest);

        Task<bool> FileExists(string fileName, StorageConfig config, string container);

        Task<string[]> DownloadCsvFileFromAutomationStorage(string fileName);
        Task<string[]> DownloadCsvFileFromMain(string fileName);
        Task<string[]> DownloadCsvFileFromCold(string fileName);
        Task<string[]> DownloadCsvFileFromEscrow(string fileName);
    }



    
    public  interface IBlobInfo
    {
        string AbsoluteUrl { get; set; }
        string FileName { get; set; }
        byte[] Contents { get; set; }
    }

    public class BlobInfo : IBlobInfo
    {
        public string AbsoluteUrl { get; set; }
        public string FileName { get; set; }
        public byte[] Contents { get; set; }
    }

    

}