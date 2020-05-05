using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Laso.Insights.FunctionalTests.Services.DataPipeline.DataQuality
{
    [TestFixture]
    [Parallelizable(ParallelScope.Fixtures)]
    public class DemographicDataQualitySchemaTests : DataPipelineTests
    {

        [Test]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesSchemaValidation))]
        public async Task ValidDemographicSchemaVariation(string fileName)
        {
            var expectedCuratedManifest = GetExpectedManifest(DataPipeline.Category.Demographic, Storage.curated,
                GetTestCsvAllCuratedExpectedMetrics());
            Category = DataPipeline.Category.Demographic.ToString();
            await DataQualityTest("dataquality/demographic/schema/validschema/", fileName, expectedCuratedManifest,
                null);
        }

        public static IEnumerable<TestCaseData> DataFilesSchemaValidation()
        {
            yield return
                new TestCaseData(
                        "Schema_AllValid_D_Demographic_20200309_20200309")
                    .SetName("Schema_AllValidTest");

            yield return
                new TestCaseData(
                        "OptData_NullCredSource_D_Demographic_20200303_20200303")
                    .SetName("SchemaNullCreditSource");

            yield return
                new TestCaseData(
                        "OptData_MissingCredScore_D_Demographic_20200303_20200303")
                    .SetName("OptData_MissingCreditScore");

            yield return
                new TestCaseData(
                        "OptData_MissingAll_D_Demographic_20200303_20200303")
                    .SetName("OptData_MissingAll");
            /*
            https://app.clubhouse.io/laso/story/4224/insights-dataquality-rows-null-optional-credit-score-are-rejected
            yield return
                new TestCaseData(
                        "OptData_NullCredScore_D_Demographic_20200303_20200303")
                    .SetName("OptData_NullCredScore");
            */
        }

        [Test]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesInvalidSchema))]
        public async Task InvalidDemographicSchemaVariation(string fileName, Manifest expectedManifestCurated,
            Manifest expectedManifestRejected)
        {
            if (expectedManifestCurated == null)
                expectedManifestCurated = GetExpectedManifest(DataPipeline.Category.Demographic, Storage.curated,
                    new Metrics
                    {
                        adjustedBoundaryRows = 0, curatedRows = 1, quality = 2, rejectedCSVRows = 0,
                        rejectedConstraintRows = 0, rejectedSchemaRows = 1, sourceRows = 2
                    });

            if (expectedManifestRejected == null)
                expectedManifestRejected = GetExpectedManifest(DataPipeline.Category.Demographic, Storage.rejected,
                    new Metrics
                    {
                        adjustedBoundaryRows = 0, curatedRows = 1, quality = 1, rejectedCSVRows = 0,
                        rejectedConstraintRows = 0, rejectedSchemaRows = 1, sourceRows = 2
                    });

            Category = DataPipeline.Category.Demographic.ToString();

            await DataQualityTest("dataquality/demographic/schema/invalidschema/", fileName, expectedManifestCurated,
                expectedManifestRejected);
        }


        public static IEnumerable<TestCaseData> DataFilesInvalidSchema()
        {
            // pass
            yield return
                new TestCaseData(
                        "ReqData_WrongType_D_Demographic_20200303_20200303",null,null)
                    .SetName("Schema_ReqDataWrongType");


            /*Done currently passing when it should fail, TODO: CSV file assertion type pending implementation
            yield return
                new TestCaseData(
                        "OptData_WrongType_D_Demographic_20200303_20200303")
                    .SetName("Schema_OptionalDataWrongType");
            //        https://app.clubhouse.io/laso/story/4211/insights-dataquality-demographic-optional-data-wrong-type-rejection-csv-file-missing-row
            */
            /*Done currently failure, expectations set for automated test case
            yield return
                new TestCaseData(
                        "ReqData_Missing_D_Demographic_20200303_20200303")
                    .SetName("Schema_ReqDataMissing");
            //https://app.clubhouse.io/laso/story/4022/insights-data-quality-demographic-rows-missing-required-data-end-up-in-curated
            */
            /*Done currently failure, expectations not set for automated test case as need agreement on expected behavior for empty rows until ticket is addressed
            yield return
            new TestCaseData(
            "EmptyRow_Laso_D_Demographic_20200305_20200305")
            .SetName("Schema_EmptyRow");
            //https://app.clubhouse.io/laso/story/4022/insights-data-quality-demographic-rows-missing-required-data-end-up-in-curated
            */
        }
        

    }
}