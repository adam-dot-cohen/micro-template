using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Queries;
using Laso.Insights.FunctionalTests.Utils;
using Microsoft.WindowsAzure.Storage.Blob;
using NUnit.Framework;

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
        protected const string _raw = "raw";
        protected const string _cold = "cold";
        protected const string _storage = "storage";
        protected const string _archive = "archive";
        public string Category = "";
        private readonly AzureBlobStg _az = new AzureBlobStg();

        protected async Task ValidPayloadTest(string folderName, string fileName, List<Manifest> manifestPayload,
            Csv expectedCsv, string extension = ".csv")
 
        {
            var waitTimeInSecs = 400;
            var fileNameDest = await CopyFileToEscrow(folderName, fileName);
            var fileBatchViewModelUntilAccepted =
                GetFileBatchViewModelUntilStatusReached(fileNameDest, "DataAccepted", true, waitTimeInSecs);

            Assert.NotNull(fileBatchViewModelUntilAccepted,
                "The corresponding File Batch View Model with an Accepted Status was not found for file  " +
                fileNameDest + " , waited " + waitTimeInSecs + " seconds");

            var blobDirectory = GetBlobDirectory(fileBatchViewModelUntilAccepted.Created);

            var blobItemsInCold =
                _az.GetFilesInBlob(ColdStorage.Key, ColdStorage.Value, AutomationPartnerId, blobDirectory);

            var blobItemInColdCsv =
                blobItemsInCold.Find(x =>
                    x.Uri.ToString().Contains(fileNameDest) && x.Uri.ToString().Contains(extension));

            var blobItemInColdManifest =
                blobItemsInCold.Find(x =>
                    x.Uri.ToString().Contains(fileBatchViewModelUntilAccepted.FileBatchId) &&
                    x.Uri.ToString().Contains(".manifest"));

            var blobItemsInRaw = _az.GetFilesInBlob(MainInsightsStorage.Key, MainInsightsStorage.Value,
                _raw, AutomationPartnerId + "/" + blobDirectory);

            var blobItemInRawCsv =
                blobItemsInRaw.Find(x =>
                    x.Uri.ToString()
                        .Contains(fileBatchViewModelUntilAccepted.FileBatchId + "_" + Category + extension));
            
            var blobItemInRawManifest =
                blobItemsInRaw.Find(x =>
                    x.Uri.ToString().Contains(fileBatchViewModelUntilAccepted.FileBatchId) &&
                    x.Uri.ToString().Contains(".manifest"));

            Assert.Multiple(() =>
            {
                Assert.NotNull(blobItemInColdCsv, "The file " + fileNameDest + " should be found in cold storage");

                Assert.NotNull(blobItemInColdManifest,
                    "The manifest file " + fileBatchViewModelUntilAccepted.FileBatchId +
                    " should be found in cold storage");
                Assert.NotNull(blobItemInRawCsv,
                    "The file " + fileBatchViewModelUntilAccepted.FileBatchId +
                    "_" + Category + extension + "  should be found in raw storage");

                Assert.NotNull(blobItemInRawManifest,
                    "The manifest file " + fileBatchViewModelUntilAccepted.FileBatchId +
                    " should be found in raw storage");
            });

            var manifestCold =
                await new AzureBlobStg().DownloadFile(ColdStorage.Key, ColdStorage.Value,
                    AutomationPartnerId,
                    blobItemInColdManifest.Uri.PathAndQuery.Replace("/" + AutomationPartnerId + "/", ""));

            Assert.NotNull(manifestCold, "The manifest in row storage should be downloaded for further verifications");

            var manifestRaw =
                await new AzureBlobStg().DownloadFile(MainInsightsStorage.Key,
                    MainInsightsStorage.Value,
                    _raw,
                    blobItemInColdManifest.Uri.PathAndQuery.Replace("/" + _raw + "/", ""));

            Assert.NotNull(manifestRaw,
                "The manifest in " + _raw + " " + _storage + " should be downloaded for further verifications");
            Assert.NotNull(manifestCold,
                "The manifest in " + _cold + " " + _storage + " should be downloaded for further verifications");

            foreach (var manifest in manifestPayload)
                manifest.correlationId = fileBatchViewModelUntilAccepted.FileBatchId;


            manifestPayload.Find(x => x.type.Equals(_raw)).documents[0].uri =
                blobItemInRawCsv.Uri.AbsoluteUri.Replace("blob", "dfs");
            manifestPayload.Find(x => x.type.Equals(_archive)).documents[0].uri = blobItemInColdCsv.Uri.AbsoluteUri;

            ManifestComparer(manifestRaw, manifestPayload.Find(x => x.type.Equals(_raw)));
            ManifestComparer(manifestCold, manifestPayload.Find(x => x.type.Equals(_archive)));

            //expectedCsv files should be just as the original in raw and escrow
            string[]  csvContentRaw =
                await new AzureBlobStg().DownloadCsvFile((CloudBlockBlob)blobItemInRawCsv);
            string[] csvContentCold =
                await new AzureBlobStg().DownloadCsvFile((CloudBlockBlob)blobItemInColdCsv);

            Csv actualCsvRaw = new Csv(((CloudBlockBlob)blobItemInRawCsv).Name,csvContentRaw);
            Csv actualCsvEscrow = new Csv(((CloudBlockBlob)blobItemInColdCsv).Name, csvContentCold);
            CsvComparer(actualCsvRaw, expectedCsv );
            CsvComparer(actualCsvEscrow, expectedCsv);

        }

        protected async Task DataQualityTest(string folderName, string fileName, DataQualityParts expectedDataCurated=null,
     DataQualityParts expectedDataRejected=null, string extension = ".csv")
        {
            var waitTimeInSecs = 600;
            var fileNameDest = await CopyFileToEscrow(folderName, fileName);
            var batchViewModelUntil =
                GetFileBatchViewModelUntilStatusReached(fileNameDest, "DataQualityComplete", false, waitTimeInSecs);
            Assert.NotNull(batchViewModelUntil,
                "The corresponding File Batch View Model with an Accepted Status was not found for file  " +
                fileNameDest + " , waited " + waitTimeInSecs + " seconds");
            var blobDirectory = GetBlobDirectory(batchViewModelUntil.Created);
            var blobItemsInCurated =
                _az.GetFilesInBlob(MainInsightsStorage.Key, MainInsightsStorage.Value, "curated",
                    AutomationPartnerId + "/" + blobDirectory);
            var blobItemInCuratedCsv =
                blobItemsInCurated.Find(x =>
                    x.Uri.ToString().Contains(batchViewModelUntil.FileBatchId + "_Demographic") &&
                    x.Uri.ToString().Contains("curated" + extension));
            var blobItemInCuratedManifest =
                blobItemsInCurated.Find(x =>
                    x.Uri.ToString().Contains(batchViewModelUntil.FileBatchId) &&
                    x.Uri.ToString().Contains(".manifest"));

            var blobItemsInRejected =
                _az.GetFilesInBlob(MainInsightsStorage.Key, MainInsightsStorage.Value, "rejected",
                    AutomationPartnerId + "/" + blobDirectory);

            var blobItemInRejectedCsv =
                blobItemsInRejected.Find(x =>
                    x.Uri.ToString().Contains(batchViewModelUntil.FileBatchId + "_Demographic") &&
                    x.Uri.ToString().Contains("rejected" + extension));
            var blobItemInRejectedManifest =
                blobItemsInRejected.Find(x =>
                    x.Uri.ToString().Contains(batchViewModelUntil.FileBatchId) &&
                    x.Uri.ToString().Contains(".manifest"));

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

                if (expectedDataRejected!=null)
                {
                    Assert.NotNull(blobItemInRejectedCsv, "The expectedCsv file should be found in rejected storage");
                    Assert.NotNull(blobItemInRejectedManifest,
                        "The manifest file  should be found in rejected storage");
                }
                else
                {
                    Assert.Null(blobItemInRejectedCsv, "The expectedCsv file should be not be found in rejected storage");
                    Assert.Null(blobItemInRejectedManifest,
                        "The manifest file  should not be found in rejected storage");
                }
            });


            Assert.Multiple(async () =>
            {
                if (expectedDataCurated != null)
                {
                    expectedDataCurated.expectedManifest.documents[0].uri =
                        blobItemInCuratedCsv.Uri.AbsoluteUri.Replace("blob", "dfs");
                    expectedDataCurated.expectedManifest.correlationId = batchViewModelUntil.FileBatchId;
                    var manifestCuratedActual =
                        await new AzureBlobStg().DownloadFile(MainInsightsStorage.Key,
                            MainInsightsStorage.Value,
                            "curated",
                            blobItemInCuratedManifest.Uri.PathAndQuery.Replace("/curated/", ""));

                    Assert.NotNull(manifestCuratedActual,
                        "The manifest in curated   should be downloaded for further verifications");
                    ManifestComparer(manifestCuratedActual, expectedDataCurated.expectedManifest);


                    string[] csvContentCurated =
                        await new AzureBlobStg().DownloadCsvFile((CloudBlockBlob)blobItemInCuratedCsv);
                    Csv csvActualCurated = new Csv(((CloudBlockBlob)blobItemInCuratedCsv).Name,csvContentCurated);

                    CsvComparer(csvActualCurated,expectedDataCurated.Csv); 



                }

                if (expectedDataRejected != null)
                {
                    expectedDataRejected.expectedManifest.documents[0].uri =
                        blobItemInRejectedCsv.Uri.AbsoluteUri.Replace("blob", "dfs");
                    expectedDataRejected.expectedManifest.correlationId = batchViewModelUntil.FileBatchId;
                        var manifestRejectedActual =
                            await new AzureBlobStg().DownloadFile(MainInsightsStorage.Key,
                                MainInsightsStorage.Value,
                                "rejected",
                                blobItemInRejectedManifest.Uri.PathAndQuery.Replace("/rejected/", ""));


                        Assert.NotNull(manifestRejectedActual,
                            "The manifest in curated   should be downloaded for further verifications");
                        ManifestComparer(manifestRejectedActual, expectedDataRejected.expectedManifest);
                        string[] csvContentRejected =
                            await new AzureBlobStg().DownloadCsvFile((CloudBlockBlob)blobItemInRejectedCsv);

                        Csv csvActualRejected = new Csv(((CloudBlockBlob)blobItemInRejectedCsv).Name, csvContentRejected);
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
                fileBatchViewModel = apis.GetFileBatchViewModel(fileName, AutomationPartnerId);
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

        public Manifest GetExpectedManifest(Category category, Storage storageType, Metrics metrics = null)
        {
            var manifest = new Manifest();

            switch (storageType)
            {
                case Storage.cold:
                    manifest.type = _archive;
                    break;
                default:
                    manifest.type = storageType.ToString();
                    break;
            }

            Category = category.ToString();
            var documentsItem = new DocumentsItem();

            if (metrics == null)
                metrics = new Metrics
                {
                    adjustedBoundaryRows = 0,
                    curatedRows = 0,
                    quality = 0,
                    rejectedCSVRows = 0,
                    rejectedConstraintRows = 0,
                    rejectedSchemaRows = 0,
                    sourceRows = 0
                };

            documentsItem.metrics = metrics;

            documentsItem.dataCategory = Category;

            manifest.tenantId = AutomationPartnerId;
            manifest.tenantName = AutomationPartnerName;
            manifest.documents = new List<DocumentsItem> {documentsItem};

            return manifest;
        }

 


        public void CsvComparer(Csv actualCsv, Csv csv)
        {
            string performValidation = "Comparing expectedCsv file :" + actualCsv.BlobCsvName + " to its baseline expectation " + csv.BlobCsvName;
            if ( actualCsv.Rows[^1].IsNullOrEmpty())
            {
                actualCsv.Rows = actualCsv.Rows.SkipLast(1).ToArray();
            }
            if (csv.Rows[^1].IsNullOrEmpty())
            {
                csv.Rows = csv.Rows.SkipLast(1).ToArray();
            }
            Assert.False(actualCsv.Rows.Length == 0, performValidation+  "There are no rows in the csv file, test cannot proceed with content validation");
            Assert.False(actualCsv.Rows.Length==1, performValidation + " There is only a header in the csv file, test cannot proceed with content validation");
            Assert.AreEqual(csv.Rows.Length, actualCsv.Rows.Length, " Test failure, the number of rows is not equal."+performValidation);
            if (actualCsv.Rows.Length == csv.Rows.Length)
            {
                var comparer = new ObjectsComparer.Comparer<string[]>();


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
            var comparer = new ObjectsComparer.Comparer<Manifest>();
            comparer.IgnoreMember("id");
            comparer.IgnoreMember("eTag");
            comparer.IgnoreMember("policy");
            comparer.IgnoreMember("events");
            comparer.IgnoreMember("orchestrationId");

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
 
        public async Task<string>  CopyFileToEscrow(string folderName, string fileName, string extension = ".csv")
        {
            var fileNameOrg = fileName + extension;
            var fileNameDest = RandomGenerator.GetRandomAlpha(5) + fileName + extension;
            CloudBlockBlob cloudBlockBlob =
            await _az.CopyFile(InsightsAutomationStorage.Key,
                InsightsAutomationStorage.Value, AutomationContainer,
                folderName + fileNameOrg, fileNameDest, EscrowStorage.Key,
                EscrowStorage.Value, "transfer-" + AutomationPartnerId);

            return fileNameDest;
        }

        //TODO:https://app.clubhouse.io/laso/story/4093/insights-payload-not-valid-need-to-reflect-validation-message-in-analysis-history
        public void InvalidPayloadVariations(string fileName)
        {
        }
    }
}