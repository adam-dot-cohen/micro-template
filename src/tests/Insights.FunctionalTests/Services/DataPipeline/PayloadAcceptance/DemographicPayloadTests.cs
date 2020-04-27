using System;
using System.Collections.Generic;
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
    Storage Account: qadatafiles
    BlobContainer: qaautomation
    TODO: Cookie for API: Not able to request one dynamically
    https://app.clubhouse.io/laso/story/4092/insights-automation-apirequests-need-to-be-able-to-generate-cookie-dynamically-to-attach-to-api-requests
    TODO: Need managed identity to get access to the required storage accounts
    https://app.clubhouse.io/laso/story/4091/insights-functional-tests-need-to-use-managed-identity-to-access-storage-accounts
    */


    [TestFixture]
    [Parallelizable(ParallelScope.Fixtures)]
    public class DemographicPayloadTests : GlobalSetUp
    {
        private const string _containerReferenceRaw = "raw";

        [Test]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesValidPayload))]
        public async Task ValidCsvPayloadVariations(string fileName)
        {
            var waitTimeInSecs = 600;
            var extension = ".csv";
            var fileNameOrg = fileName + extension;
            var fileNameDest = RandomGenerator.GetRandomAlpha(5) + fileName + extension;

            await new AzureBlobStg().CopyFile(InsightsAutomationStorage.Key,
                InsightsAutomationStorage.Value, "qaautomation",
                "payload/" + fileNameOrg, fileNameDest, EscrowStorage.Key,
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
                _containerReferenceRaw, AutomationPartnerId + "/" + blobDirectory);

            var blobItemInRawCsv =
                blobItemsInRaw.Find(x =>
                    x.Uri.ToString()
                        .Contains(fileBatchViewModelUntilAccepted.FileBatchId + "_Demographic" + extension));
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
                    "_Demographic.csv  should be found in raw storage");

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
                    _containerReferenceRaw,
                    blobItemInColdManifest.Uri.PathAndQuery.Replace("/" + _containerReferenceRaw + "/", ""));

            Assert.NotNull(manifestRaw, "The manifest in row storage should be downloaded for further verifications");
            Assert.NotNull(manifestCold, "The manifest in cold storage should be downloaded for further verifications");

            //manifest validations:
            ManifestAssertions(manifestCold, "cold storage");
            ManifestAssertions(manifestRaw, "raw storage");
        }


        public void ManifestAssertions(Manifest manifest, string storageType)
        {
            var storageTypeValidation = "Validating manifest in " + storageType;
            Assert.Multiple(() =>
            {
                Assert.AreEqual(manifest.documents.Count, 1,
                    "There should be one document listed in the " + storageTypeValidation);
                Assert.AreEqual(manifest.documents[0].dataCategory, "Demographic",
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

                if (storageType.Equals("cold storage"))
                {
                    Assert.AreEqual( "archive", manifest.type,
                        "Validating manifest type in  " + storageTypeValidation);
                }
                else
                {
                    Assert.AreEqual("raw", manifest.type,
                        "Validating manifest type in  " + storageTypeValidation);

                }

            });
        }

        public static IEnumerable<TestCaseData> DataFilesValidPayload()
        {
            yield return
                new TestCaseData(
                        "AllValidCsv_Laso_D_Demographic_20200415_20200415")
                    .SetName("DemographicDataAllFieldsInFormat");
            yield return
                new TestCaseData("AllValidCsv_Laso_W_Demographic_20200415_20200415")
                    .SetName("DemographicDataWeeklyFrequency");
            yield return
                new TestCaseData("AllValidCsv_Laso_M_Demographic_20200415_20200415")
                    .SetName("DemographicDataMonthlyFrequency");
            yield return
                new TestCaseData("AllValidCsv_Laso_Q_Demographic_20200415_20200415")
                    .SetName("DemographicDataQuarterlyFrequency");
            yield return
                new TestCaseData("AllValidCsv_Laso_Y_Demographic_20200415_20200415")
                    .SetName("DemographicDataYearlyFrequency");
            yield return
                new TestCaseData("AllValidCsv_Laso_R_Demographic_20200415_20200415")
                    .SetName("DemographicDataOnRequestFrequency");
            /*
            yield return
                new TestCaseData("Empty Space_Laso_Y_Demographic_20200415_20200415")
                    .SetName("PayloadEmptySpace"); 
            //TODO: Add when this ticket is addressed: https://app.clubhouse.io/laso/story/4031/insights-payload-file-names-with-empty-spaces-fail-copying-to-escrow
            //TODO: Need to understand the limit in name length, go right on the max limit
            //https://app.clubhouse.io/laso/story/4045/data-processing-error-when-file-name-is-too-long
            */
        }


        [Test]
        [Ignore("Cannot validate the validation message errors in api analysis history")]
        //https://app.clubhouse.io/laso/story/4093/insights-payload-not-valid-need-to-reflect-validation-message-in-analysis-history
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesInvalidPayload))]
        public void InvalidCsvPayloadVariations(string fileName)
        {
        }


        public static IEnumerable<TestCaseData> DataFilesInvalidPayload()
        {
            yield return
                new TestCaseData(
                        "AllValidCsv_Laso_Invalid_Demographic_20200415_20200415.csv")
                    .SetName("DemographicDataInvalidFrequency");

            yield return
                new TestCaseData(
                        "UnknownCategory_Laso_W_emographic_20200415_20200415.csv")
                    .SetName("UnknownCategory");

            yield return
                new TestCaseData(
                        "NoFormat.csv")
                    .SetName("NotAValidFormat");

            yield return
                new TestCaseData(
                        "UknExtension_Laso_Y_Demographic_20200415_20200415.tr")
                    .SetName("UnknownExtenstion");

            //TODO: Need to understand the limit in name length, go past the limit
            //https://app.clubhouse.io/laso/story/4045/data-processing-error-when-file-name-is-too-long
            yield return
                new TestCaseData(
                        "asdfasfsafsfdsafsaf_asdfsafasfabtgryrtytertafasf_D_Demographic_202003006_20200306.csv")
                    .SetName("PayloadFileNameExceedsLenghtLimit");
        }


        private FileBatchViewModel GetFileBatchViewModelUntilAccepted(string fileName, string status,
            int waiTimeSpanInSeconds)
        {
            var waitSecBetweenCalls = 10;
            var apis = new ApiRequests();

            for (var i = 0; i < waiTimeSpanInSeconds; i++)
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

                Thread.Sleep(TimeSpan.FromSeconds(waitSecBetweenCalls));
                i = i + waitSecBetweenCalls;
            }

            return null;
        }
    }
}