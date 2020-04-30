using System.Collections.Generic;
using System.Threading.Tasks;
using Laso.Insights.FunctionalTests.Services.DataPipeline.PayloadAcceptance;
using NUnit.Framework;

namespace Laso.Insights.FunctionalTests.Services.DataPipeline.DataQuality
{
    [TestFixture]
    [Parallelizable(ParallelScope.Fixtures)]
    public class DemographicDataQualityTests : DataPipelineTests
    {
        [Test]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesCsvValidation))]
        public async Task ValidDemographicCsvVariation(string fileName)
        {
            var expectedCuratedManifest = GetExpectedManifest(DataPipeline.Category.Demographic, Storage.curated,
                GetTestValidCsvExpectedMetrics());
            Category = DataPipeline.Category.Demographic.ToString();
            await DataQualityTest("dataquality/demographic/csv/validcsv/", fileName, expectedCuratedManifest, null);
        }

        public static IEnumerable<TestCaseData> DataFilesCsvValidation()
        {
            yield return
                new TestCaseData(
                        "ValidCsvMatch_Laso_D_Demographic_20200415_20200415")
                    .SetName("ValidCsvDemographicExactMatch");

            yield return
                new TestCaseData("ValidCsv_AllUpper_D_Demographic_20200304_20200304")
                    .SetName("ValidCsvDemographicAllUpperCase");
            yield return
                new TestCaseData("ValidCsv_AllLower_D_Demographic_20200304_20200304")
                    .SetName("ValidCsvDemographicAllLowerCase");
            yield return
                new TestCaseData("ValidCsv_CamelCase_D_Demographic_20200304_20200304")
                    .SetName("ValidCsvDemographicCamelCase");
        }


        [Test]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesInvalidCsv))]
        public async Task InvalidDemographicCsvVariation(string fileName, List<string> errorListInRejectedManifest,
            bool noHeader = false)
        {
            var expectedManifestRejected = GetExpectedManifest(DataPipeline.Category.Demographic, Storage.rejected,
                !noHeader ? GetTestInvalidCsvExpectedMetrics() : GetTestInvalidCsvNoHeaderExpectedMetrics());

            expectedManifestRejected.documents[0].errors = errorListInRejectedManifest;

            Category = DataPipeline.Category.Demographic.ToString();

            await DataQualityTest("dataquality/demographic/csv/invalidcsv/", fileName, null, expectedManifestRejected);
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

            yield return
                new TestCaseData("Csv_IncorrectOrder_D_Demographic_20200304_20200304",
                        new List<string>
                        {
                            "RULE CSV.2 - Header column mismatch CREDIT_SCORE:BRANCH_ID",
                            "RULE CSV.2 - Header column mismatch BRANCH_ID:CREDIT_SCORE"
                        }, false)
                    .SetName("InvalidCsvIncorrectOrder");

            yield return
                new TestCaseData("ExtraColumnsMiddle_Laso_D_Demographic_20200308_20200308",
                        new List<string> {"RULE CSV.2 - Header column mismatch EXTRACOLUMN:ClientKey_id"}, false)
                    .SetName("InvalidCsvExtraColumnsMiddle");

            yield return
                new TestCaseData("NoHeader_Laso_D_Demographic_20200310_20200310",
                        new List<string>
                        {
                            "RULE CSV.2 - Header column mismatch Demographic_Data:LASO_CATEGORY",
                            "RULE CSV.2 - Header column mismatch 119386:ClientKey_id",
                            "RULE CSV.2 - Header column mismatch 501:BRANCH_ID",
                            "RULE CSV.2 - Header column mismatch 750:CREDIT_SCORE",
                            "RULE CSV.2 - Header column mismatch NULL:CREDIT_SCORE_SOURCE"
                        }, true)
                    .SetName("InvalidCsvNoHeader");
        }

        public Metrics GetTestInvalidCsvNoHeaderExpectedMetrics()
        {
            return new Metrics
            {
                adjustedBoundaryRows = 0,
                curatedRows = 0,
                quality = 0,
                rejectedCSVRows = 2,
                rejectedConstraintRows = 0,
                rejectedSchemaRows = 0,
                sourceRows = 2
            };
        }

        public Metrics GetTestInvalidCsvExpectedMetrics()
        {
            return new Metrics
            {
                adjustedBoundaryRows = 0,
                curatedRows = 0,
                quality = 0,
                rejectedCSVRows = 3,
                rejectedConstraintRows = 0,
                rejectedSchemaRows = 0,
                sourceRows = 3
            };
        }

        public Metrics GetTestValidCsvExpectedMetrics()
        {
            return new Metrics
            {
                adjustedBoundaryRows = 0,
                curatedRows = 2,
                quality = 2,
                rejectedCSVRows = 0,
                rejectedConstraintRows = 0,
                rejectedSchemaRows = 0,
                sourceRows = 2
            };
        }
    }
}