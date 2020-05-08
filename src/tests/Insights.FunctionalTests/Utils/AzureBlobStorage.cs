using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Laso.Insights.FunctionalTests.Services.DataPipeline;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace Laso.Insights.FunctionalTests.Utils
{
    public class AzureBlobStg

    {
        public Task<bool> FileExists(string fileName, string blobStorageAccount, string key, string container)
        {
            var cloudStorageAccount = new CloudStorageAccount(
                new StorageCredentials(blobStorageAccount,
                    key), true);

            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            var cloudBobContainer = cloudBlobClient.GetContainerReference(container);
            return cloudBobContainer.GetBlockBlobReference(fileName).ExistsAsync();
        }


        public List<IListBlobItem> GetFilesInBlob(string storage, string keyVal, string container, string directory)
        {
            CloudStorageAccount cloudStorageAccount = null;
            CloudBlobContainer cloudBlobContainer = null;
            CloudBlobClient cloudBlobClient = null;
            cloudStorageAccount = new CloudStorageAccount(
                new StorageCredentials(storage,
                    keyVal),
                true);
            cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            cloudBlobContainer = cloudBlobClient.GetContainerReference(container);

            var dirRef = cloudBlobContainer.GetDirectoryReference(directory);

            var res = dirRef.ListBlobsSegmentedAsync(null);

            return res.Result.Results.ToList();
        }

        public async Task<Manifest> DownloadFile(string storageAcct, string storageAcctKey, string container,
            string fileName)

        {
            var cloudStorageAccountOrg = new CloudStorageAccount(
                new StorageCredentials(storageAcct, storageAcctKey), true);

            var cloudBlobClientOrg = cloudStorageAccountOrg.CreateCloudBlobClient();


            var cloudBobContainerOrigin = cloudBlobClientOrg.GetContainerReference(container);

            var blockBlob =
                cloudBobContainerOrigin.GetBlockBlobReference(fileName);


            var fileExists =
                await blockBlob.ExistsAsync();


            if (!fileExists)
            {
                throw new Exception("File " +blockBlob.Uri+" does not exist ");
            }

            string text;
            await using (var memoryStream = new MemoryStream())
            {
                await blockBlob.DownloadToStreamAsync(memoryStream);
                text = Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            return
                JsonConvert.DeserializeObject<Manifest>(text);
        }

        public async Task<string[]> DownloadCsvFile(CloudBlockBlob blockBlob)
        {
           

            var fileExists =
                await blockBlob.ExistsAsync();


            if (!fileExists)
            {
                throw new Exception("File " + blockBlob.Uri + " does not exist ");
            }

            string[] lines;
            // List<string> lines = new List<string>();
            string text;
            await using (var memoryStream = new MemoryStream())
            {
                await blockBlob.DownloadToStreamAsync(memoryStream);
                text = Encoding.UTF8.GetString(memoryStream.ToArray());
                lines = text.Split(
                    new[] { Environment.NewLine, "\r", "\n" },
                    StringSplitOptions.None
                );

            }

            return lines;

        }

        public async Task<string[]>   DownloadCsvFileFromAutomationStorage
        (
        string fileNameOrg)

        {
            var cloudStorageAccountOrg = new CloudStorageAccount(
                 new StorageCredentials(GlobalSetUp.InsightsAutomationStorage.Key, GlobalSetUp.InsightsAutomationStorage.Value), true);

             var cloudBlobClientOrg = cloudStorageAccountOrg.CreateCloudBlobClient();


             var cloudBobContainerOrigin = cloudBlobClientOrg.GetContainerReference(GlobalSetUp.AutomationContainer);

             var blockBlob =
                 cloudBobContainerOrigin.GetBlockBlobReference(fileNameOrg);


             var fileExists =
                 await blockBlob.ExistsAsync();


             if (!fileExists)
             {
                 throw new Exception("File " + blockBlob.Uri + " does not exist ");
             }

            string[] lines;
           
            string text;
            await using (var memoryStream = new MemoryStream())
            {
                await blockBlob.DownloadToStreamAsync(memoryStream);
                text = Encoding.UTF8.GetString(memoryStream.ToArray());
                lines = text.Split(
                    new[] { Environment.NewLine,  "\r","\n"  },
                    StringSplitOptions.None
                );

            }

            return lines;

        }


        public async Task<MemoryStream> DownloadStream(string storageAcct, string storageAcctKey, string container,
            string fileName)

        {
            var cloudStorageAccountOrg = new CloudStorageAccount(
                new StorageCredentials(storageAcct, storageAcctKey), true);

            var cloudBlobClientOrg = cloudStorageAccountOrg.CreateCloudBlobClient();


            var cloudBobContainerOrigin = cloudBlobClientOrg.GetContainerReference(container);

            var blockBlob =
                cloudBobContainerOrigin.GetBlockBlobReference(fileName);


            var fileExists =
                await blockBlob.ExistsAsync();


            if (!fileExists)
            {
                throw new Exception("File " + blockBlob.Uri + " does not exist ");
            }

            MemoryStream memoryStream = new MemoryStream();
                await blockBlob.DownloadToStreamAsync(memoryStream);
            return memoryStream;

        }

        public async Task<CloudBlockBlob> CopyFile(string storageAcctOrigin, string storageAcctKeyOrigin, string containerOrigin,
            string fileNameOrg, string fileNameDest, string blobStorageAccountDestination, string blobKeyDestination,
            string blobContainerDestination)

        {
            var cloudStorageAccount = new CloudStorageAccount(
                new StorageCredentials(blobStorageAccountDestination, blobKeyDestination), true);

            var cloudStorageAccountOrg = new CloudStorageAccount(
                new StorageCredentials(storageAcctOrigin, storageAcctKeyOrigin), true);

            var cloudBlobClientOrg = cloudStorageAccountOrg.CreateCloudBlobClient();
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            var cloudBobContainerOrigin = cloudBlobClientOrg.GetContainerReference(containerOrigin);

            var cloudBobContainerDest = cloudBlobClient.GetContainerReference(blobContainerDestination);
            var cloudBlockBlobOrigin =
                cloudBobContainerOrigin.GetBlockBlobReference(fileNameOrg);
            var
                cloudBlockBlobDest =
                    cloudBobContainerDest.GetBlockBlobReference("incoming/" + fileNameDest);


            await using var targetBlobStream = await cloudBlockBlobDest.OpenWriteAsync();
            await using var sourceBlobStream = await cloudBlockBlobOrigin.OpenReadAsync();
            await sourceBlobStream.CopyToAsync(targetBlobStream);

            sourceBlobStream.Dispose();
            targetBlobStream.Dispose();

            var fileExists =
                await cloudBobContainerDest.GetBlockBlobReference("incoming/" + fileNameDest).ExistsAsync();


            if (!fileExists)
                throw new Exception("File " + fileNameDest + " was not copied successfully to " +
                                    blobStorageAccountDestination + " " + blobContainerDestination);

            return cloudBobContainerDest.GetBlockBlobReference("incoming/" + fileNameDest);
        }
    }
}