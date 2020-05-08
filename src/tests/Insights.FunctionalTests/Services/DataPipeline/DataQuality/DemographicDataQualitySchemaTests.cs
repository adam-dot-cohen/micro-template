using System.Collections.Generic;
using System.Threading.Tasks;
using Laso.Insights.FunctionalTests.Utils;
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
        public async Task ValidDemographicSchemaVariation(string folderName,string fileName,DataQualityParts dataQualityPartsCurated)
        {

            string csvBaseline = folderName + fileName + ".csv";

            string[] expectedRows =
                new AzureBlobStg()
                    .DownloadCsvFileFromAutomationStorage(csvBaseline).Result;

            Csv csv = new Csv(csvBaseline, expectedRows);
            dataQualityPartsCurated.Csv = csv;
            await DataQualityTest(folderName, fileName, dataQualityPartsCurated,
                null);
        }
       
        public static IEnumerable<TestCaseData> DataFilesSchemaValidation()
        {
            string folderName = "dataquality/demographic/schema/validschema/";
            yield return
                new TestCaseData(folderName, "Schema_AllValid_D_Demographic_20200309_20200309",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            DataPipeline.Category.Demographic,
                            Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), null)) 
                    .SetName("Schema_AllValidTest");
            yield return
                new TestCaseData(folderName, "OptData_NullCredSource_D_Demographic_20200303_20200303",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            DataPipeline.Category.Demographic,
                            Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), null))
                    .SetName("SchemaNullCreditSource");
            yield return
                new TestCaseData(folderName, "OptData_MissingCredScore_D_Demographic_20200303_20200303",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            DataPipeline.Category.Demographic,
                            Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), null))
                    .SetName("OptData_MissingCreditScore");
            yield return
                new TestCaseData(folderName, "OptData_MissingAll_D_Demographic_20200303_20200303",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            DataPipeline.Category.Demographic,
                            Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), null))
                    .SetName("OptData_MissingAll");

                     /*   https://app.clubhouse.io/laso/story/4224/insights-dataquality-rows-null-optional-credit-score-are-rejected
                        yield return
                            new TestCaseData(
                                    "OptData_NullCredScore_D_Demographic_20200303_20200303")
                                .SetName("OptData_NullCredScore");
                        */

        }

        [Test]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesInvalidSchema))]
        public async Task InvalidDemographicSchemaVariation(string folderName,string fileName, DataQualityParts expectedDataCurated=null,
            DataQualityParts expectedDataRejected=null)
        {
            await DataQualityTest(folderName, fileName, expectedDataCurated,
                expectedDataRejected);
        }

        public static IEnumerable<TestCaseData> DataFilesInvalidSchema()
        {
            string folderName = "dataquality/demographic/schema/invalidschema/";
             //pass
             yield return
                 new TestCaseData(folderName, "ReqData_WrongType_D_Demographic_20200303_20200303",
                         new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                             DataPipeline.Category.Demographic,
                             Storage.curated, new Metrics
                             {
                                 adjustedBoundaryRows = 0,
                                 curatedRows = 1,
                                 quality = 2,
                                 rejectedCSVRows = 0,
                                 rejectedConstraintRows = 0,
                                 rejectedSchemaRows = 1,
                                 sourceRows = 2
                             }), new Csv(folderName+ "ReqData_WrongType_D_Demographic_curated.baseline")),
                         new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                                 DataPipeline.Category.Demographic,
                                 Storage.rejected, new Metrics
                                 {
                                     adjustedBoundaryRows = 0,
                                     curatedRows = 1,
                                     quality = 1,
                                     rejectedCSVRows = 0,
                                     rejectedConstraintRows = 0,
                                     rejectedSchemaRows = 1,
                                     sourceRows = 2}), new Csv(folderName + "ReqData_WrongType_D_Demographic_rejected.baseline")))
                     .SetName("Schema_ReqDataWrongType");

             //https://app.clubhouse.io/laso/story/4211/insights-dataquality-demographic-optional-data-wrong-type-rejection-csv-file-missing-row
             yield return
                 new TestCaseData(folderName, "OptData_WrongType_D_Demographic_20200303_20200303",
                         new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                             DataPipeline.Category.Demographic,
                             Storage.curated, new Metrics
                             {
                                 adjustedBoundaryRows = 0,
                                 curatedRows = 1,
                                 quality = 2,
                                 rejectedCSVRows = 0,
                                 rejectedConstraintRows = 0,
                                 rejectedSchemaRows = 1,
                                 sourceRows = 2
                             }), new Csv(folderName + "OptData_WrongType_D_Demographic_curated.baseline")),
                         new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                             DataPipeline.Category.Demographic,
                             Storage.rejected, new Metrics
                             {
                                 adjustedBoundaryRows = 0,
                                 curatedRows = 1,
                                 quality = 1,
                                 rejectedCSVRows = 0,
                                 rejectedConstraintRows = 0,
                                 rejectedSchemaRows = 1,
                                 sourceRows = 2
                             }), new Csv(folderName + "OptData_WrongType_D_Demographic_rejected.baseline")))
                     .SetName("Schema_OptDataWrongType");
                     
            //https://app.clubhouse.io/laso/story/4022/insights-data-quality-demographic-rows-missing-required-data-end-up-in-curated
            yield return
                new TestCaseData(folderName, "ReqData_Missing_D_Demographic_20200303_20200303",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            DataPipeline.Category.Demographic,
                            Storage.curated, new Metrics
                            {
                                adjustedBoundaryRows = 0,
                                curatedRows = 1,
                                quality = 2,
                                rejectedCSVRows = 0,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 1,
                                sourceRows = 2
                            }), new Csv(folderName + "ReqData_Missing_D_Demographic_curated.baseline")),
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            DataPipeline.Category.Demographic,
                            Storage.rejected, new Metrics
                            {
                                adjustedBoundaryRows = 0,
                                curatedRows = 1,
                                quality = 1,
                                rejectedCSVRows = 0,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 1,
                                sourceRows = 2
                            }), new Csv(folderName + "ReqData_Missing_D_Demographic_rejected.baseline")))
                    .SetName("Schema_ReqDataMissing");
            //https://app.clubhouse.io/laso/story/4022/insights-data-quality-demographic-rows-missing-required-data-end-up-in-curated

            //Pending ExtraData_Laso_D_Demographic_20200309_20200309.csv
            //Extra data
            //falls into the header category(like an additional column empty header)....question I think I need to open another ticket
            //https://app.clubhouse.io/laso/story/4174/insights-data-quality-no-rejected-files-created-in-rejected-file-system

            //Pending Empty row:EmptyRow_Laso_D_Demographic_20200305_20200305.csv
            //Expected the empty row to be rejected: https://app.clubhouse.io/laso/story/4022/insights-data-quality-demographic-rows-missing-required-data-end-up-in-curated
        }


 

    }
    }