using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using NUnit.Framework;
using Shouldly;

namespace Laso.Insights.FunctionalTests.Utils
{
    [TestFixture, Ignore("Tests for utility debug")]
    public class AzureStorageTests
    {
        [Test] 
        public void GetFilesInBlobDir()
        {
            IAzureBlobStg _az =
                new AzureBlobStgFactory().Create();

            var storageConfig = new StorageConfig
            {
                Key = "",
                Account = "lasodevinsightscold"
            };

    
            string sourceFile = "2020/202006/20200617/152747_Schema_AllValid_R_AccountTransaction_20191029_20191029095900.csv";


            var source = new BlobMeta
            {
                FileName = sourceFile,
                ContainerName = "84644678-bd17-4210-b4d6-50795d3e1794",
                Config = storageConfig
            };

            List<BlobItem> biList =
            _az.GetFilesInBlob(storageConfig, source.ContainerName, "2020/202006/20200617");
            biList.Count.ShouldBeGreaterThanOrEqualTo(1);
            
        }


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


        [Test] 
        public async Task TestCopySameResourcegGroupDifferentStorageAccounts()
        {
            IAzureBlobStg _az =
                new AzureBlobStgFactory().Create();

            var storageConfig = new StorageConfig
            {
                Key = "",
                Account = "lasodevinsightscold"
            };

            var storageConfigEscrow = new StorageConfig
            {
                Key = "",
                Account = "lasodevinsightsescrow"
            };

            string sourceFile = "2020/202006/20200617/152747_Schema_AllValid_R_AccountTransaction_20191029_20191029095900.csv";
            string destFile =  RandomGenerator.GetRandomAlpha(5);


            var source = new BlobMeta
            {
                FileName = sourceFile,
                ContainerName = "84644678-bd17-4210-b4d6-50795d3e1794",
                Config = storageConfig
            };

            var dest = new BlobMeta
            {
                Config = storageConfigEscrow,
                FileName = destFile,
                ContainerName = "transfer-84644678-bd17-4210-b4d6-50795d3e1794"
            };

            bool sourceFileExits =
                await _az.FileExists(sourceFile, storageConfig, source.ContainerName);

            Assert.True(sourceFileExits, "file in " + source.ContainerName + "exists");

            await _az.CopyFile(source, dest);

            Assert.True(
                await _az.FileExists(destFile, storageConfigEscrow, dest.ContainerName), "File created");


        }

        [Test] 
        public async Task TestCopyDifferentStorageAccounts()
        {
            IAzureBlobStg _az =
                new AzureBlobStgFactory().Create();

            var storageConfig = new StorageConfig
            {
                Key = "",
                Account = "qainsightsautomation"
            };

            var storageConfigEscrow = new StorageConfig
            {
                Key = "",
                Account = "lasodevinsightsescrow"
            };

            string sourceFile = "payload/accounttransactions/validpayload/AllValid_Laso_D_AccountTransaction_20201029_20190427.csv";
            string destFile = "incoming/" + RandomGenerator.GetRandomAlpha(5);


            var source = new BlobMeta
            {
                FileName = sourceFile,
                ContainerName = "qaautomation",
                Config = storageConfig
            };

            var dest = new BlobMeta
            {
                Config = storageConfigEscrow,
                FileName = destFile,
                ContainerName = "transfer-84644678-bd17-4210-b4d6-50795d3e1794"
            };

            bool sourceFileExits =
                await _az.FileExists(sourceFile, storageConfig, source.ContainerName);

            Assert.True(sourceFileExits, "file in " + source.ContainerName + "exists");

            await _az.CopyFile(source, dest);

            Assert.True(
                await _az.FileExists(destFile, storageConfigEscrow, dest.ContainerName), "File created");


        }

        [Test] 
        public async Task TestCopySameStorageAccountsDifferentContainer()
        {
            IAzureBlobStg _az =
                new AzureBlobStgFactory().Create();

            var storageConfig = new StorageConfig
            {
                Key = "",
                Account = "qainsightsautomation"
            };

            string sourceFile = "payload/accounttransactions/validpayload/AllValid_Laso_D_AccountTransaction_20201029_20190427.csv";
            string destFile = "incoming/" + RandomGenerator.GetRandomAlpha(5);


            var source = new BlobMeta
            {
                FileName = sourceFile,
                ContainerName = "qaautomation",
                Config = storageConfig
            };

            var dest = new BlobMeta
            {
                Config = storageConfig,
                FileName = destFile,
                ContainerName = "qadata"
            };

            bool sourceFileExits =
                await _az.FileExists(sourceFile, storageConfig, source.ContainerName);

            Assert.True(sourceFileExits, "file in " + source.ContainerName + "exists");

            await _az.CopyFile(source, dest);

            Assert.True(
            await _az.FileExists(destFile, storageConfig, dest.ContainerName),"File created");


        }


    }
}