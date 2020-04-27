using System;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Monitoring.DataQualityPipeline.Queries;
using Laso.Insights.FunctionalTests.Utils;
using NUnit.Framework;

namespace Laso.Insights.FunctionalTests.Services.DataPipeline.PayloadAcceptance
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
    */


    [TestFixture]
    [Parallelizable(ParallelScope.Fixtures)]
    public abstract class PayloadAcceptanceTests : GlobalSetUp
    {
        protected const string _raw = "raw";
        protected const string _cold = "cold";
        protected const string _storage = "storage";
        protected const string _archive = "archive";
        public string Category = "";

        protected async Task ValidPayloadTest(string folderName, string fileName, string extension = ".csv")
        {
            var waitTimeInSecs = 600;
            var fileNameOrg = fileName + extension;
            var fileNameDest = RandomGenerator.GetRandomAlpha(5) + fileName + extension;

            await new AzureBlobStg().CopyFile(InsightsAutomationStorage.Key,
                InsightsAutomationStorage.Value, AutomationContainer,
                folderName + fileNameOrg, fileNameDest, EscrowStorage.Key,
                EscrowStorage.Value, "transfer-" + AutomationPartnerId);

            var fileBatchViewModelUntilAccepted =
                GetFileBatchViewModelUntilAccepted(fileNameDest, "DataAccepted", waitTimeInSecs);
            Assert.NotNull(fileBatchViewModelUntilAccepted,
                "The corresponding File Batch View Model with an Accepted Status was not found for file  " +
                fileNameDest + " , waited " + waitTimeInSecs + " seconds");
            var dirInBlobContainer = fileBatchViewModelUntilAccepted.Created;
            var dateT = dirInBlobContainer.ToString("MM/dd/yyyy");
            var dirs = dateT.Split("/");
            var blobDirectory = dirs[2] + "/" + dirs[2] + dirs[0] + "/" + dirs[2] + dirs[0] + dirs[1];
            var az = new AzureBlobStg();

            var blobItemsInCold =
                az.GetFilesInBlob(ColdStorage.Key, ColdStorage.Value, AutomationPartnerId, blobDirectory);

            var blobItemInColdCsv =
                blobItemsInCold.Find(x =>
                    x.Uri.ToString().Contains(fileNameDest) && x.Uri.ToString().Contains(extension));

            var blobItemInColdManifest =
                blobItemsInCold.Find(x =>
                    x.Uri.ToString().Contains(fileBatchViewModelUntilAccepted.FileBatchId) &&
                    x.Uri.ToString().Contains(".manifest"));

            var blobItemsInRaw = az.GetFilesInBlob(MainInsightsStorage.Key, MainInsightsStorage.Value,
                _raw, AutomationPartnerId + "/" + blobDirectory);

            var blobItemInRawCsv =
                blobItemsInRaw.Find(x =>
                    x.Uri.ToString()
                        .Contains(fileBatchViewModelUntilAccepted.FileBatchId + "_"+Category + extension));
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
                    "_"+Category+extension+"  should be found in raw storage");

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

           Assert.NotNull(manifestRaw, "The manifest in "+_raw+" "+_storage+" should be downloaded for further verifications");
           Assert.NotNull(manifestRaw, "The manifest in " + _cold + " " + _storage + " should be downloaded for further verifications");

            //manifest validations:
            ManifestAssertions(manifestCold, _cold +" "+_storage);
            ManifestAssertions(manifestRaw, _raw + " "+_storage);
        }


        public void ManifestAssertions(Manifest manifest, string storageType)
        {
            var storageTypeValidation = "Validating manifest in " + storageType;
            Assert.Multiple(() =>
            {
                Assert.AreEqual(manifest.documents.Count, 1,
                    "There should be one document listed in the " + storageTypeValidation);
                Assert.AreEqual(manifest.documents[0].dataCategory, Category,
                    "Validating category " + storageTypeValidation);
                Assert.AreEqual(manifest.tenantId, AutomationPartnerId,
                    "Validating partner id / tenant id  " + storageTypeValidation);
                Assert.AreEqual(manifest.tenantName, AutomationPartnerName,
                    "Validating partner name  " + storageTypeValidation);
                Assert.AreEqual(0, manifest.documents[0].metrics.adjustedBoundaryRows,
                    "Validating adjusted boundary rows in " + storageType);
                Assert.AreEqual(0, manifest.documents[0].metrics.curatedRows,
                    "Validating curated rows in " + storageType);
                Assert.AreEqual(0, manifest.documents[0].metrics.quality, "Validating quality in " + storageType);
                Assert.AreEqual(0, manifest.documents[0].metrics.rejectedCSVRows,
                    "Validating rejected csv rows " + storageType);
                Assert.AreEqual(0, manifest.documents[0].metrics.rejectedConstraintRows,
                    "Validating rejected constraint rows " + storageType);
                Assert.AreEqual(0, manifest.documents[0].metrics.rejectedSchemaRows,
                    "Validating rejected schema rows " + storageType);
                Assert.AreEqual(0, manifest.documents[0].metrics.sourceRows,
                    "Validating source rows " + storageType);

                if (storageType.Equals(_cold+" "+_storage))
                {
                    Assert.AreEqual( _archive, manifest.type,
                        "Validating manifest type in  " + storageTypeValidation);
                }
                else
                {
                    Assert.AreEqual(_raw, manifest.type,
                        "Validating manifest type in  " + storageTypeValidation);

                }

            });
        }

        public void InvalidPayloadVariations(string fileName)
        {
        }


 
        private FileBatchViewModel GetFileBatchViewModelUntilAccepted(string fileName, string status,
            int waiTimeSpanInSeconds)
        {
            var waitSecBetweenReqs = 10;
            var apis = new ApiRequests();

            for (var i = 0; i < waiTimeSpanInSeconds; i = i + waitSecBetweenReqs)
            {
                FileBatchViewModel fileBatchViewModel = null;
                fileBatchViewModel = apis.GetFileBatchViewModel(fileName, AutomationPartnerId);
                if (fileBatchViewModel != null && fileBatchViewModel.Status.Equals("DataAccepted"))
                {
                    return fileBatchViewModel;
                }

                if (fileBatchViewModel != null)
                    Console.WriteLine("FileBatchFileViewModel for " + fileName + " is not null, current status is " +
                                      fileBatchViewModel.Status + ". Waiting for status to be " + status);

                Thread.Sleep(TimeSpan.FromSeconds(waitSecBetweenReqs));
            }

            return null;
        }
    }
}