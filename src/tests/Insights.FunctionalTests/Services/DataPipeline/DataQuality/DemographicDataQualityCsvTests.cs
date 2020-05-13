using System.Collections.Generic;
using System.Threading.Tasks;
using Laso.Insights.FunctionalTests.Utils;
using NUnit.Framework;

namespace Laso.Insights.FunctionalTests.Services.DataPipeline.DataQuality
{
    [TestFixture]
    [Parallelizable(ParallelScope.Fixtures)]
    public class DemographicDataQualityCsvTests : DataPipelineTests
    {
        [Test]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesCsvValidation))]
        public async Task ValidDemographicCsvVariation(string folderName, string fileName,
            DataQualityParts expectedCurated, DataQualityParts expectedRejected)
        {
            await DataQualityTest(folderName, fileName, expectedCurated,null);
        }

        public static IEnumerable<TestCaseData> DataFilesCsvValidation()
        {
            var folderName = "dataquality/demographic/csv/validcsv/";
            string csvBaseline = "ValidCsvMatch_Laso_D_Demographic_20200415_20200415.csv"; string[] expectedRows = new AzureBlobStg()
                  .DownloadCsvFileFromAutomationStorage(folderName +
                                                      csvBaseline)
                .Result;
            Csv csv = new Csv(csvBaseline,expectedRows);

            yield return
                new TestCaseData(folderName,
                        "ValidCsvMatch_Laso_D_Demographic_20200415_20200415", 
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            DataPipeline.Category.Demographic,
                            Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), csv), null)
                    .SetName("ValidCsvDemographicExactMatch");
            yield return
                new TestCaseData(folderName, 
                        "ValidCsv_AllUpper_D_Demographic_20200304_20200304",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            DataPipeline.Category.Demographic,
                            Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), csv), null)
                    .SetName("ValidCsvDemographicAllUpperCase");
            yield return
                new TestCaseData(folderName,
                        "ValidCsv_AllLower_D_Demographic_20200304_20200304",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            DataPipeline.Category.Demographic,
                            Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), csv), null)
                    .SetName("ValidCsvDemographicAllLowerCase");
            yield return
                new TestCaseData(folderName, 
                        "ValidCsv_CamelCase_D_Demographic_20200304_20200304",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            DataPipeline.Category.Demographic,
                            Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), csv), null)
                     .SetName("ValidCsvDemographicCamelCase");
        }

        [Test]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesInvalidCsv))]
        public async Task InvalidDemographicCsvVariation(string folderName, string fileName,
            DataQualityParts expectedCurated, DataQualityParts expectedRejected,
            List<string> errorListInRejectedManifest)
        {

            string csvBaseline = folderName + fileName + ".csv";

            string[] expectedRows =
            new AzureBlobStg()
                .DownloadCsvFileFromAutomationStorage(folderName + fileName + ".csv").Result;
                
            Csv csv = new Csv(csvBaseline, expectedRows);

            expectedRejected.expectedManifest.documents[0].errors = errorListInRejectedManifest;
            expectedRejected.Csv = csv;
            await DataQualityTest(folderName, fileName, expectedCurated, expectedRejected);
        }

        public static IEnumerable<TestCaseData> DataFilesInvalidCsv()
        {
            /*
             FOLLOWING 4 TEST CASES:
             https://app.clubhouse.io/laso/story/4174/insights-data-quality-no-rejected-files-created-in-rejected-file-system
            yield return
                new TestCaseData("ExtraColumnsBeg_Laso_D_Demographic_20200306_20200306",null,expectedRejectedManifest)
                    .SetName("InvalidCsvExtraColumnsBeginning");
            yield return
                new TestCaseData("ExtraColumnsEnd_Laso_D_Demographic_20200307_20200307",null,expectedRejectedManifest)
                    .SetName("InvalidCsvExtraColumnsEnd");
            yield return
                new TestCaseData("Csv_MissReqColumn_D_Demographic_20200306_20200306",null,expectedRejectedManifest)
                    .SetName("InvalidCsvMissRequiredColumn");
            yield return
                new TestCaseData(
                        "WrongCategory_Laso_D_AccountTransaction_20200303_20200303", new List<string>() { "RULE CSV.2 - Header column mismatch Demographic_Data:LASO_CATEGORY", "RULE CSV.2 - Header column mismatch 119386:ClientKey_id", "RULE CSV.2 - Header column mismatch 501:BRANCH_ID", "RULE CSV.2 - Header column mismatch 750:CREDIT_SCORE", "RULE CSV.2 - Header column mismatch NULL:CREDIT_SCORE_SOURCE" }, false)
                    .SetName("InvalidCsvWrongCategory");

                    */


            var folderName = "dataquality/demographic/csv/invalidcsv/";
            yield return
                new TestCaseData(folderName, "Csv_IncorrectOrder_D_Demographic_20200304_20200304", null, 
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            DataPipeline.Category.Demographic,
                            Storage.rejected, new ExpectedMetrics().GetTestCsvAllRejectedExpectedMetrics(3)), null),

                        new List<string>
                        {
                            "RULE CSV.2 - Header column mismatch CREDIT_SCORE:BRANCH_ID",
                            "RULE CSV.2 - Header column mismatch BRANCH_ID:CREDIT_SCORE"
                        })
                    .SetName("InvalidCsvIncorrectOrder");

            yield return
                new TestCaseData(folderName, "ExtraColumnsMiddle_Laso_D_Demographic_20200308_20200308", null,
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            DataPipeline.Category.Demographic,
                            Storage.rejected, new ExpectedMetrics().GetTestCsvAllRejectedExpectedMetrics(3)), null),
                        new List<string> {"RULE CSV.2 - Header column mismatch EXTRACOLUMN:ClientKey_id"})
                    .SetName("InvalidCsvExtraColumnsMiddle");

            yield return
                new TestCaseData(folderName, "NoHeader_Laso_D_Demographic_20200310_20200310", null,
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
                            Storage.rejected, new ExpectedMetrics().GetTestCsvAllRejectedExpectedMetrics()), null),
                        new List<string>
                        {
                            "RULE CSV.2 - Header column mismatch Demographic_Data:LASO_CATEGORY",
                            "RULE CSV.2 - Header column mismatch 119386:ClientKey_id",
                            "RULE CSV.2 - Header column mismatch 501:BRANCH_ID",
                            "RULE CSV.2 - Header column mismatch 750:CREDIT_SCORE",
                            "RULE CSV.2 - Header column mismatch NULL:CREDIT_SCORE_SOURCE"
                        })
                    .SetName("InvalidCsvNoHeader");
        }
    }
}