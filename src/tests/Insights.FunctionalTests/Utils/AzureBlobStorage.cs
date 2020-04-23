using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

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


        public async Task CopyFile(string storageAcctOrigin, string storageAcctKeyOrigin, string containerOrigin, string fileNameOrg, string fileNameDest, string blobStorageAccountDestination, string blobKeyDestination,
            string blobContainerDestination)

        {var cloudStorageAccount = new CloudStorageAccount(
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
                cloudBlockBlobdest =
                    cloudBobContainerDest.GetBlockBlobReference("incoming/" + fileNameDest);


            await using var targetBlobStream = await cloudBlockBlobdest.OpenWriteAsync();
            await using var sourceBlobStream = await cloudBlockBlobOrigin.OpenReadAsync();
            await sourceBlobStream.CopyToAsync(targetBlobStream);

            sourceBlobStream.Dispose();
            targetBlobStream.Dispose();

           bool fileExists = await cloudBobContainerDest.GetBlockBlobReference("incoming/" + fileNameDest).ExistsAsync();
            

            if (!fileExists)
            {
                throw new Exception("File "+fileNameDest +" was not copied successfully to "+ blobStorageAccountDestination+ " "+blobContainerDestination);
            }
            
        }
    }
}