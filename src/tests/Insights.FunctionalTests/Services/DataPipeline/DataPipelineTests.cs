using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using IdentityServer4.Extensions;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Queries;
using Laso.Insights.FunctionalTests.Utils;
using NUnit.Framework;
using ObjectsComparer;

namespace Laso.Insights.FunctionalTests.Services.DataPipeline
{
    /*
    Current setup in develop: Partner called AutoBank
    test with a provided partner: 84644678-bd17-4210-b4d6-50795d3e1794
    Storage Account: qainsightsautomation
    BlobContainer: qaautomation
    TODO: Cookie for API: Not able to request one dynamically
    https://app.clubhouse.io/laso/story/4092/insights-automation-apirequests-need-to-be-able-to-generate-cookie-dynamically-to-attach-to-api-requests
    TODO: Need managed identity to get access to the required storage accounts
    https://app.clubhouse.io/laso/story/4091/insights-functional-tests-need-to-use-managed-identity-to-access-storage-accounts
    TODO: Update manifest expected uris when the defect below is addressed
    https://app.clubhouse.io/laso/story/4202/insights-dataquality-manifest-uri-data-contains-dfs-instead-of-blob
    */


    [TestFixture]
    [Parallelizable(ParallelScope.Fixtures)]
    public abstract class DataPipelineTests : GlobalSetUp
    {
        private const string Raw = "raw";
        private const string Cold = "cold";
        private const string Curated = "curated";
        private const string Rejected = "rejected";
        private const string Storage = "storage";

        private readonly IAzureBlobStg _az = new AzureBlobStgFactory().Create();

        /**
         * This ValidPayloadTest focuses on validations when files are 
         * Files are uploaded to escrow, and validations are performed after the batch reaches
         * status of DataAccepted.
         * Validations performed are as follows:
         * Copies of the files in cold and raw storage, as well as existence of their manifests.
         * Csv content and manifest content validation is performed.         
         */

        protected async Task<FileBatchViewModel> ValidPayloadTest(string folderName, string fileName,
            DataQualityParts dataQualityPartsRaw,
            DataQualityParts dataQualityPartsCold, string extension = ".csv")

        {
            var category = dataQualityPartsRaw.expectedManifest.documents[0].dataCategory;
            var waitTimeInSecs = 400;
            var fileNameDest = await CopyFileToEscrow(folderName, fileName);
            var fileBatchViewModelUntilAccepted =
                GetFileBatchViewModelUntilStatusReached(fileNameDest, "DataAccepted", true, waitTimeInSecs);

            Assert.NotNull(fileBatchViewModelUntilAccepted,
                "The corresponding File Batch View Model with an Accepted Status was not found for file  " +
                fileNameDest + " , waited " + waitTimeInSecs + " seconds");

            var blobDirectory = GetBlobDirectory(fileBatchViewModelUntilAccepted.Created);

            var blobItemsInCold =
                _az.GetFilesInBlob(TestConfiguration.ColdStorage, TestConfiguration.AutomationPartner.Id, blobDirectory);

            var blobItemInColdCsv =
                blobItemsInCold.Find(x =>
                    x.Name.Contains(fileNameDest) 
                    && x.Name.ToString().Contains(extension));

            var blobItemInColdManifest =
                blobItemsInCold.Find(x =>
                    x.Name.Contains(fileBatchViewModelUntilAccepted.FileBatchId) &&
                    x.Name.Contains(".manifest"));

            var blobItemsInRaw = _az.GetFilesInBlob(TestConfiguration.MainInsightsStorage, Raw, TestConfiguration.AutomationPartner.Id + "/" + blobDirectory);

            var blobItemInRawCsv =
                blobItemsInRaw.Find(x =>
                    x.Name.ToString()
                        .Contains(fileBatchViewModelUntilAccepted.FileBatchId + "_" + category + extension));

            var blobItemInRawManifest =
                blobItemsInRaw.Find(x =>
                    x.Name.ToString().Contains(fileBatchViewModelUntilAccepted.FileBatchId) &&
                    x.Name.ToString().Contains(".manifest"));

            Assert.Multiple(() =>
            {
                Assert.NotNull(blobItemInColdCsv, "The file " + fileNameDest + " should be found in cold storage");

                Assert.NotNull(blobItemInColdManifest,
                    "The manifest file " + fileBatchViewModelUntilAccepted.FileBatchId +
                    " should be found in cold storage");
                Assert.NotNull(blobItemInRawCsv,
                    "The file " + fileBatchViewModelUntilAccepted.FileBatchId +
                    "_" + category + extension + "  should be found in raw storage");

                Assert.NotNull(blobItemInRawManifest,
                    "The manifest file " + fileBatchViewModelUntilAccepted.FileBatchId +
                    " should be found in raw storage");
            });

            var azureBlobStg = new AzureBlobStgFactory().Create();


            var manifestCold =
                await azureBlobStg.DownloadFile(TestConfiguration.ColdStorage, TestConfiguration.AutomationPartner.Id,
                    blobItemInColdManifest.Name.Replace("/" + TestConfiguration.AutomationPartner.Id + "/", ""));

            var manifestRaw =
                await azureBlobStg.DownloadFile(TestConfiguration.MainInsightsStorage, Raw,
                    blobItemInRawManifest.Name.Replace("/" + Raw + "/", ""));

            Assert.Multiple(() =>
            {
                Assert.NotNull(manifestRaw,
                    "The manifest in " + Raw + " " + Storage + " should be downloaded for further verifications");
                Assert.NotNull(manifestCold,
                    "The manifest in " + Cold + " " + Storage + " should be downloaded for further verifications");
            });
            dataQualityPartsCold.expectedManifest.correlationId = fileBatchViewModelUntilAccepted.FileBatchId;
            dataQualityPartsRaw.expectedManifest.correlationId = fileBatchViewModelUntilAccepted.FileBatchId;

            dataQualityPartsRaw.expectedManifest.documents[0].uri =
                blobItemInRawCsv.Name.Replace("blob", "dfs");

            dataQualityPartsCold.expectedManifest.documents[0].uri = blobItemInColdCsv.Name;


            //expectedCsv files should be just as the original in raw and escrow
            var csvContentRaw =
                await azureBlobStg.DownloadCsvFile(blobItemInRawCsv.Name, TestConfiguration.MainInsightsStorage,Raw);
            var csvContentCold =
                await _az.DownloadCsvFile(blobItemInColdCsv.Name,TestConfiguration.ColdStorage, GlobalSetUp.TestConfiguration.AutomationPartner.Id);

            var actualCsvRaw = new Csv(blobItemInRawCsv.Name, csvContentRaw);
            var actualCsvEscrow = new Csv(blobItemInColdCsv.Name, csvContentCold);

            Assert.Multiple(() =>
            {
                ManifestComparer(manifestRaw, dataQualityPartsRaw.expectedManifest);
                ManifestComparer(manifestCold, dataQualityPartsCold.expectedManifest);
                CsvComparer(actualCsvRaw, dataQualityPartsRaw.Csv);
                CsvComparer(actualCsvEscrow, dataQualityPartsCold.Csv);
            });
            return fileBatchViewModelUntilAccepted;
        }

        /**
         * This DataQualityTest can be used after ValidPayloadTest where the FileBatchViewModel is returned.
         * This test case is to be used to test all details of the data pipeline"
         */
        protected async Task DataQualityTest(FileBatchViewModel fileBatchViewModel,
            DataQualityParts expectedDataCurated = null,
            DataQualityParts expectedDataRejected = null, string extension = ".csv")
        {
            await DataQualityTest("", "", expectedDataCurated, expectedDataRejected, extension, fileBatchViewModel);
        }

        /**
         * This DataQualityTest focuses on validating the DataQuality pipeline.
         * Files are uploaded to escrow, and validations are performed after the batch reaches
         * status of DataQualityComplete.
         * Validations performed are as follows:
         * Csv files existence in curated and rejected storage, as well as existence of their manifests as defined by the test expectations
         * Csv content and manifest content validation is performed as defined by test expectations
         */

        protected async Task DataQualityTest(string folderName, string fileName,
            DataQualityParts expectedDataCurated = null,
            DataQualityParts expectedDataRejected = null, string extension = ".csv",
            FileBatchViewModel fileBatchViewModelAlreadyAccepted = null)
        {
            string category;
            var fileNameDest = "";
            FileBatchViewModel batchViewModelUntil = null;
            var waitTimeInSecs = 600;
            if (fileBatchViewModelAlreadyAccepted == null)
            {
                fileNameDest = await CopyFileToEscrow(folderName, fileName);
                batchViewModelUntil =
                    GetFileBatchViewModelUntilStatusReached(fileNameDest, "DataQualityComplete", false, waitTimeInSecs);
            }
            else

            {
                fileNameDest = fileBatchViewModelAlreadyAccepted.Files.ToArray()[0].Filename;
                batchViewModelUntil =
                    GetFileBatchViewModelUntilStatusReached(fileNameDest, "DataQualityComplete", false, 300);
            }

            if (expectedDataCurated != null)
                category = expectedDataCurated.expectedManifest.documents[0].dataCategory;
            else
                category = expectedDataRejected.expectedManifest.documents[0].dataCategory;

            Assert.NotNull(batchViewModelUntil,
                "The corresponding File Batch View Model with DataQualityComplete status was not found for file  " +
                fileNameDest + " , waited " + waitTimeInSecs + " seconds");
            var blobDirectory = GetBlobDirectory(batchViewModelUntil.Created);
            var blobItemsInCurated =
                _az.GetFilesInBlob(TestConfiguration.MainInsightsStorage, Curated,
                    TestConfiguration.AutomationPartner.Id + "/" + blobDirectory);
            var blobItemInCuratedCsv =
                blobItemsInCurated.Find(x =>
                    x.Name.ToString().Contains(batchViewModelUntil.FileBatchId + "_" + category) &&
                    x.Name.ToString().Contains(Curated + extension));
            var blobItemInCuratedManifest =
                blobItemsInCurated.Find(x =>
                    x.Name.ToString().Contains(batchViewModelUntil.FileBatchId) &&
                    x.Name.ToString().Contains(".manifest"));

            var blobItemsInRejected =
                _az.GetFilesInBlob(TestConfiguration.MainInsightsStorage, Rejected,
                    TestConfiguration.AutomationPartner.Id + "/" + blobDirectory);

            var blobItemInRejectedCsv =
                blobItemsInRejected.Find(x =>
                    x.Name.ToString().Contains(batchViewModelUntil.FileBatchId + "_" + category) &&
                    x.Name.ToString().Contains(Rejected + extension));
            var blobItemInRejectedManifest =
                blobItemsInRejected.Find(x =>
                    x.Name.ToString().Contains(batchViewModelUntil.FileBatchId) &&
                    x.Name.ToString().Contains(".manifest"));

            Assert.Multiple(() =>
            {
                if (expectedDataCurated != null)
                {
                    Assert.NotNull(blobItemInCuratedCsv, "The expectedCsv file should be found in curated storage");
                    Assert.NotNull(blobItemInCuratedManifest, "The manifest file  should be found in curated storage");
                }
                else
                {
                    Assert.Null(blobItemInCuratedCsv, "The expectedCsv file should be not be found in curated storage");
                    Assert.Null(blobItemInCuratedManifest,
                        "The manifest file  should not be found in curated storage");
                }

                if (expectedDataRejected != null)
                {
                    Assert.NotNull(blobItemInRejectedCsv, "The expectedCsv file should be found in rejected storage");
                    Assert.NotNull(blobItemInRejectedManifest,
                        "The manifest file  should be found in rejected storage");
                }
                else
                {
                    Assert.Null(blobItemInRejectedCsv,
                        "The expectedCsv file should be not be found in rejected storage");
                    Assert.Null(blobItemInRejectedManifest,
                        "The manifest file  should not be found in rejected storage");
                }
            });


            Assert.Multiple(async () =>
            {
                if (expectedDataCurated != null)
                {
                    expectedDataCurated.expectedManifest.documents[0].uri =
                        blobItemInCuratedCsv.Name.Replace("blob", "dfs");
                    expectedDataCurated.expectedManifest.correlationId = batchViewModelUntil.FileBatchId;
                    var azureBlobStg = new AzureBlobStgFactory().Create();


                    var manifestCuratedActual =
                        await azureBlobStg.DownloadFile(TestConfiguration.MainInsightsStorage, "curated",
                            blobItemInCuratedManifest.Name.Replace("/curated/", ""));

                    Assert.NotNull(manifestCuratedActual,
                        "The manifest in curated   should be downloaded for further verifications");
                    ManifestComparer(manifestCuratedActual, expectedDataCurated.expectedManifest);


                    var csvContentCurated =
                        await azureBlobStg.DownloadCsvFile(blobItemInCuratedCsv.Name, TestConfiguration.MainInsightsStorage, Curated);
                    //var itemInCuratedCsv = Encoding.UTF8.GetString(blobItemInCuratedCsv.DownloadFile(null).Contents);

                    var csvActualCurated = new Csv(blobItemInCuratedCsv.Name, csvContentCurated);

                    CsvComparer(csvActualCurated, expectedDataCurated.Csv);
                }

                if (expectedDataRejected != null)
                {
                    expectedDataRejected.expectedManifest.documents[0].uri =
                        blobItemInRejectedCsv.Name.Replace("blob", "dfs");
                    expectedDataRejected.expectedManifest.correlationId = batchViewModelUntil.FileBatchId;
                    var manifestRejectedActual =
                        await _az.DownloadFile(TestConfiguration.MainInsightsStorage,
                            "rejected",
                            blobItemInRejectedManifest.Name.Replace("/rejected/", ""));


                    Assert.NotNull(manifestRejectedActual,
                        "The manifest in curated   should be downloaded for further verifications");
                    ManifestComparer(manifestRejectedActual, expectedDataRejected.expectedManifest);
                    var csvContentRejected =
                        await _az.DownloadCsvFile(blobItemInRejectedCsv.Name, TestConfiguration.MainInsightsStorage, Rejected);

                    var csvActualRejected = new Csv(blobItemInRejectedCsv.Name, null);
                    CsvComparer(csvActualRejected, expectedDataRejected.Csv);
                }
            });
        }


        private FileBatchViewModel GetFileBatchViewModelUntilStatusReached(string fileName, string status,
            bool statusOnBatch,
            int waiTimeSpanInSeconds)
        {
            var waitSecBetweenReqs = 10;
            var apis = new ApiRequests();

            for (var i = 0; i < waiTimeSpanInSeconds; i = i + waitSecBetweenReqs)
            {
                FileBatchViewModel fileBatchViewModel = null;
                fileBatchViewModel = apis.GetFileBatchViewModel(fileName, TestConfiguration.AutomationPartner.Id);
                if (statusOnBatch)
                {
                    if (fileBatchViewModel != null && fileBatchViewModel.Status.Equals(status))
                        return fileBatchViewModel;
                }
                else
                {
                    if (fileBatchViewModel != null &&
                        fileBatchViewModel.ProductAnalysisRuns.Any(x => x.Statuses.Any(y => y.Status.Equals(status))))
                        return fileBatchViewModel;
                }

                if (fileBatchViewModel != null)

                    Console.WriteLine("FileBatchFileViewModel for " + fileName + " is not null, current status is " +
                                      fileBatchViewModel.Status + ". Waiting for status to be " + status);

                Thread.Sleep(TimeSpan.FromSeconds(waitSecBetweenReqs));
            }

            return null;
        }


        public void CsvComparer(Csv actualCsv, Csv csv)
        {
            var performValidation = "Comparing expectedCsv file :" + actualCsv.BlobCsvName +
                                    " to its baseline expectation " + csv.BlobCsvName;
            if (actualCsv.Rows[^1].IsNullOrEmpty()) actualCsv.Rows = actualCsv.Rows.SkipLast(1).ToArray();
            if (csv.Rows[^1].IsNullOrEmpty()) csv.Rows = csv.Rows.SkipLast(1).ToArray();
            Assert.False(actualCsv.Rows.Length == 0,
                performValidation + "There are no rows in the csv file, test cannot proceed with content validation");
            Assert.False(actualCsv.Rows.Length == 1,
                performValidation +
                " There is only a header in the csv file, test cannot proceed with content validation");
            Assert.AreEqual(csv.Rows.Length, actualCsv.Rows.Length,
                " Test failure, the number of rows is not equal." + performValidation);
            if (actualCsv.Rows.Length == csv.Rows.Length)
            {
                var comparer = new Comparer<string[]>();


                var isEqual = comparer.Compare(actualCsv.Rows, csv.Rows,
                    out var differences);
                var dif = string.Join(Environment.NewLine, differences);
                dif = dif.Replace("Value1", "ActualCsv ").Replace("Value2", "Csv");
                Assert.True(isEqual,
                    performValidation + " resulted in differences " +
                    dif);
            }
        }

        public void ManifestComparer(Manifest manifestActual, Manifest manifestExpected)
        {
            var comparer = new Comparer<Manifest>();
            comparer.IgnoreMember("id");
            comparer.IgnoreMember("eTag");
            comparer.IgnoreMember("policy");
            comparer.IgnoreMember("events");
            comparer.IgnoreMember("orchestrationId");
            comparer.IgnoreMember("uri");

            var isEqual = comparer.Compare(manifestActual, manifestExpected,
                out var differences);
            var dif = string.Join(Environment.NewLine, differences);
            dif = dif.Replace("Value1", "ActualManifestIn " + manifestExpected.type).Replace("Value2", "Expected");
            Assert.True(isEqual,
                "The comparison between the expected and actual values for the object type " +
                manifestExpected.GetType().Name + " resulted in differences " +
                dif);
        }

        private string GetBlobDirectory(DateTimeOffset fileBatchCreated)
        {
            var dateT = fileBatchCreated.ToString("MM/dd/yyyy");
            var dirs = dateT.Split("/");
            return dirs[2] + "/" + dirs[2] + dirs[0] + "/" + dirs[2] + dirs[0] + dirs[1];
        }

        public async Task<string> CopyFileToEscrow(string folderName, string fileName, string extension = ".csv")
        {
            var fileNameOrg = fileName + extension;
            var fileNameDest = RandomGenerator.GetRandomAlpha(5) + fileName + extension;

            var source = new BlobMeta
            {
                Config = TestConfiguration.AutomationStorage,
                ContainerName = TestConfiguration.AutomationPartner.Container,
                FileName = $"{folderName}{fileNameOrg}"
            };
            var dest= new BlobMeta
            {

                Config = TestConfiguration.EscrowStorage,
                ContainerName = $"transfer-{TestConfiguration.AutomationPartner.Id}",
                FileName = "incoming/" + fileNameDest
                
            };

            await _az.CopyFile(source,dest);

            return fileNameDest;
        }

        //TODO:https://app.clubhouse.io/laso/story/4093/insights-payload-not-valid-need-to-reflect-validation-message-in-analysis-history
        public void InvalidPayloadVariations(string fileName)
        {
        }
    }
}