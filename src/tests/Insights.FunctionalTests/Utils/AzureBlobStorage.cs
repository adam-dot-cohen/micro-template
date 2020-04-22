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
            //this is the storage dedicated space

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


        public async Task CopyFile(string fileNameOrg, string fileNameDest, string blobStorageAccount, string blobKey,
            string blobDestination, string containerOrigin = "qaautomation")

        {
            var cloudStorageAccount = new CloudStorageAccount(
                new StorageCredentials(blobStorageAccount, blobKey), true);


            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            CloudBlobContainer cloudBobContainerOrigin;
            cloudBobContainerOrigin =
                cloudBlobClient.GetContainerReference(containerOrigin);

            CloudBlobContainer cloudBobContainerDest;
            cloudBobContainerDest =
                cloudBlobClient.GetContainerReference(blobDestination);
            var cloudBlockBlobOrigin =
                cloudBobContainerOrigin.GetBlockBlobReference(fileNameOrg);
            var
                cloudBlockBlobdest =
                    cloudBobContainerDest.GetBlockBlobReference("incoming/" + fileNameDest);
            await cloudBlockBlobdest.StartCopyAsync(cloudBlockBlobOrigin);
        }
    }
}