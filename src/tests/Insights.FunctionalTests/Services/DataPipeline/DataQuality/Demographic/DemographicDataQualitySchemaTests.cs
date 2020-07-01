using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Laso.Insights.FunctionalTests.Services.DataPipeline.DataQuality.Demographic
{
    [TestFixture]
    [Parallelizable(ParallelScope.Fixtures)]
    public class DemographicDataQualitySchemaTests : DataPipelineTests
    {
        [Test, Timeout(720000)]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesSchemaValidation))]
        public async Task ValidDemographicSchemaVariation(string folderName, string fileName,
            DataQualityParts dataQualityPartsCurated)
        {
            var csvBaseline = folderName + fileName + ".csv";

            var csv = new Csv(csvBaseline);
            dataQualityPartsCurated.Csv = csv;
            await DataQualityTest(folderName, fileName, dataQualityPartsCurated);
        }

        public static IEnumerable<TestCaseData> DataFilesSchemaValidation()
        {
            var folderName = "dataqualityv4/demographic/schema/validschema/";
            yield return
                new TestCaseData(folderName, "Schema_AllValid_D_Demographic_20200309_20200309",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
                            Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), null))
                    .SetName("Schema_AllValidTest");
        }

        [Test, Timeout(720000)]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesInvalidSchema))]
        public async Task InvalidDemographicSchemaVariation(string folderName, string fileName,
            DataQualityParts expectedDataCurated = null,
            DataQualityParts expectedDataRejected = null)
        {
            await DataQualityTest(folderName, fileName, expectedDataCurated,
                expectedDataRejected);
        }

        public static IEnumerable<TestCaseData> DataFilesInvalidSchema()
        {
            var folderName = "dataqualityv4/demographic/schema/invalidschema/";

            //FIXED and UPDATED VALIDATED 06/29 https://app.clubhouse.io/laso/story/4211/insights-dataquality-demographic-optional-data-wrong-type-rejection-csv-file-missing-row
            yield return
                new TestCaseData(folderName, "ReqData_WrongType_D_Demographic_20200303_20200303",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
                            Storage.curated, new Metrics
                            {
                                adjustedBoundaryRows = 0,
                                curatedRows = 1,
                                quality = 2,
                                rejectedCSVRows = 0,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 1,
                                sourceRows = 2
                            }), new Csv(folderName + "ReqData_WrongType_D_Demographic_curated.baseline")),
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
                            Storage.rejected, new Metrics
                            {
                                adjustedBoundaryRows = 0,
                                curatedRows = 1,
                                quality = 1,
                                rejectedCSVRows = 0,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 1,
                                sourceRows = 2
                            }), new Csv(folderName + "ReqData_WrongType_D_Demographic_rejected.baseline")))
                    .SetName("Schema_ReqDataWrongType");

            //06/29 still an issue
            //https://app.clubhouse.io/laso/story/4022/insights-data-quality-demographic-rows-missing-required-data-end-up-in-curated
            yield return
                new TestCaseData(folderName, "ReqData_MissingIds_D_Demographic_20200303_20200303",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
                            Storage.curated, new Metrics
                            {
                                adjustedBoundaryRows = 0,
                                curatedRows = 1,
                                quality = 2,
                                rejectedCSVRows = 0,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 1,
                                sourceRows = 2
                            }), new Csv(folderName + "ReqData_MissingIds_D_Demographic_curated.baseline")),
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
                            Storage.rejected, new Metrics
                            {
                                adjustedBoundaryRows = 0,
                                curatedRows = 1,
                                quality = 1,
                                rejectedCSVRows = 0,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 1,
                                sourceRows = 2
                            }), new Csv(folderName + "ReqData_MissingIds_D_Demographic_rejected.baseline")))
                    .SetName("Schema_ReqDataMissingIds");

            //https://app.clubhouse.io/laso/story/4356/insights-dataquality-in-all-rejected-rows-curated-files-are-created
            yield return
                new TestCaseData(folderName, "ReqData_NullCredScore_D_Demographic_20200303_20200303",
                        null,
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
                            Storage.rejected, new Metrics
                            {
                                adjustedBoundaryRows = 0,
                                curatedRows = 0,
                                quality = 1,
                                rejectedCSVRows = 0,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 2,
                                sourceRows = 2
                            }), new Csv(folderName + "ReqData_NullCredScore_D_Demographic_rejected.baseline")))
                    .SetName("ReqData_NullCredScore");


            //https://app.clubhouse.io/laso/story/4022/insights-data-quality-demographics-rows-missing-required-data-end-up-in-curated
            yield return
                new TestCaseData(folderName, "ReqData_CredScore_D_Demographic_20200303_20200303",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
                            Storage.curated, new Metrics
                            {
                                adjustedBoundaryRows = 0,
                                curatedRows = 2,
                                quality = 2,
                                rejectedCSVRows = 0,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 3,
                                sourceRows = 5
                            }), new Csv(folderName + "ReqData_CreditScore_D_Demographic_curated.baseline")),
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
                            Storage.rejected, new Metrics
                            {
                                adjustedBoundaryRows = 0,
                                curatedRows = 2,
                                quality = 1,
                                rejectedCSVRows = 0,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 3,
                                sourceRows = 5
                            }), new Csv(folderName + "ReqData_CreditScore_D_Demographic_rejected.baseline")))
                    .SetName("Schema_ReqCreditScore");


            //https://app.clubhouse.io/laso/story/4363/insights-dataquality-demographic-credit-source-rows-with-invalid-credit-source-values-get-curated
            yield return
                new TestCaseData(folderName, "ReqData_CredSource_D_Demographic_20200303_20200303",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
                            Storage.curated, new Metrics
                            {
                                adjustedBoundaryRows = 0,
                                curatedRows = 4,
                                quality = 2,
                                rejectedCSVRows = 0,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 3,
                                sourceRows = 7
                            }), new Csv(folderName + "ReqData_CredSource_D_Demographic_curated.baseline")),
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
                            Storage.rejected, new Metrics
                            {
                                adjustedBoundaryRows = 0,
                                curatedRows = 4,
                                quality = 1,
                                rejectedCSVRows = 0,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 3,
                                sourceRows = 7
                            }), new Csv(folderName + "ReqData_CredSource_D_Demographic_rejected.baseline")))
                    .SetName("Schema_ReqCreditSource");


            //https://app.clubhouse.io/laso/story/4022/insights-data-quality-demographics-rows-missing-required-data-end-up-in-curated
            //NOTE: This test case has a variation with row with no data and also new line
            yield return
                new TestCaseData(folderName, "EmptyRow_Laso_D_Demographic_20200305_20200305",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
                            Storage.curated, new Metrics
                            {
                                adjustedBoundaryRows = 0,
                                curatedRows = 2,
                                quality = 2,
                                rejectedCSVRows = 0,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 1,
                                sourceRows = 3
                            }), new Csv(folderName + "EmptyRow_Laso_D_Demographic_curated.baseline")),
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
                            Storage.rejected, new Metrics
                            {
                                adjustedBoundaryRows = 0,
                                curatedRows = 2,
                                quality = 1,
                                rejectedCSVRows = 0,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 1,
                                sourceRows = 3
                            }), new Csv(folderName + "EmptyRow_Laso_D_Demographic_rejected.baseline")))
                    .SetName("Schema_EmptyRows");
        }
    }
}