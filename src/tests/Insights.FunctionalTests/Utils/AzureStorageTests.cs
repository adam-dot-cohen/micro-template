using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

namespace Laso.Insights.FunctionalTests.Utils
{
    public class AzureStorageTests
    {

        [Test]
        public void TestExists()
        {

            var client = new AzureBlobStgFactory().Create();

            var storageConfig = new StorageConfig
            {
                Key = "",
                Account = "adblobtesting"
            };



            client.FileExists("__dbs-main__.py",storageConfig, "test").Result.ShouldBeTrue();

        }


        
        [Test]
        public async Task TestCopy()
        {

            var client = new AzureBlobStgFactory().Create();

            var storageConfig = new StorageConfig
            {
                Key = "",
                Account = "adblobtesting"
            };
            string sourceFile = "__dbs-main__.py";
            string destFile = $"__dbs-main{new Random().Next(10000000)}__.py";

            
            var source = new BlobMeta
            {
                FileName = sourceFile,
                ContainerName = "test",
                Config = storageConfig
            };

            var dest= new BlobMeta
            {
                Config = storageConfig,
                FileName = destFile,
                ContainerName = "test"
            };

            await  client.CopyFile(source,dest);

            
            var fileCheck = await client.FileExists(destFile,storageConfig, "test");
            fileCheck.ShouldBeTrue();



        }


    }
}