using System.Collections.Generic;
using System.Threading.Tasks;
using Laso.Insights.FunctionalTests.Utils;
using NUnit.Framework;

namespace Laso.Insights.FunctionalTests.Services.DataPipeline.AccountTransactions
{
    [TestFixture]
    [Parallelizable(ParallelScope.Fixtures)]
    public class AccountTransactionsDataQualitySchemaTests : DataPipelineTests
    {
        [Test]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesSchemaValidation))]
        public async Task ValidAccountTransactionsSchemaVariation(string folderName, string fileName,
            DataQualityParts dataQualityPartsCurated)
        {
            if (dataQualityPartsCurated.Csv == null)
            {
                var csvBaseline = folderName + fileName + ".csv";

                var expectedRows =
                    new AzureBlobStg()
                        .DownloadCsvFileFromAutomationStorage(csvBaseline).Result;

                var csv = new Csv(csvBaseline, expectedRows);
                dataQualityPartsCurated.Csv = csv;
            }

            await DataQualityTest(folderName, fileName, dataQualityPartsCurated);
        }

        public static IEnumerable<TestCaseData> DataFilesSchemaValidation()
        {
            var folderName = "dataquality/accounttransactions/schema/validschema/";


            yield return
                new TestCaseData(folderName, "Schema_AllValid_R_AccountTransaction_20191029_20191029095900",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), null))
                    .SetName("Schema_AllValid_AllData");
            yield return
                new TestCaseData(folderName, "Schema_DateFormatDashD_R_AccountTransaction_20191029_20191029",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), null))
                    .SetName("Schema_DateFormatMM-DD");

            yield return
                new TestCaseData(folderName, "Schema_DateFormatDashS_R_AccountTransaction_20191029_20191029",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                                Category.AccountTransaction,
                                Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()),
                            new Csv(folderName + "Schema_DateFormatDashD_R_AccountTransaction_20191029_20191029.csv")))
                    .SetName("Schema_DateFormatM-D");
            yield return
                new TestCaseData(folderName, "Schema_MissingOptCategory_R_AccountTransaction_20191029_20191029",
                    new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                        Category.AccountTransaction,
                        Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), null));
            yield return
                new TestCaseData(folderName, "Schema_OnlyRequired_R_AccountTransaction_20191029_20191029095900",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), null))
                    .SetName("Schema_OnlyRequired");
            /*
             FOLLOWING 4 TEST CASES:
            //DEFECT https://app.clubhouse.io/laso/story/4271/insights-data-quality-account-transactions-valid-date-format-results-in-rejected-rows-no-data-in-csv

        yield return
            new TestCaseData(folderName, "Schema_DateFormatSlashD_R_AccountTransaction_20191029_20191029",
            new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                Category.AccountTransaction,
                Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), null))
        .SetName("Schema_DateFormatMM/DD");
        

             yield return
                 new TestCaseData(folderName, "Schema_DateFormatSlashS_R_AccountTransaction_20191029_20191029",
                         new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                             Category.AccountTransaction,
                             Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), null))
                     .SetName("Schema_DateFormatM/D");

                         yield return
                 new TestCaseData(folderName, "Schema_DateFormatSlashS_R_AccountTransaction_20191029_20191029",
                         new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                             Category.AccountTransaction,
                             Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), null))
                     .SetName("Schema_DateFormatM/D");

                add more variations when defect gets addressed: 
                Schema_DateForVarDashD_R_AccountTransaction_20191029_20191029. 
                Schema_DateForVarSlashS_R_AccountTransaction_20191029_20191029

              */
        }

        //AllException the TestActionAttribute cases fail
        // https://app.clubhouse.io/laso/story/4283/automation-account-transactions-missing-required-data-ends-in-curated-storage-and-data-dictionary-spec-needs-revision

        [Test]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesInvalidSchema))]
        public async Task InvalidAccountTransactionSchemaVariation(string folderName, string fileName,
            DataQualityParts expectedDataCurated = null,
            DataQualityParts expectedDataRejected = null)
        {
            await DataQualityTest(folderName, fileName, expectedDataCurated,
                expectedDataRejected);
        }

        public static IEnumerable<TestCaseData> DataFilesInvalidSchema()
        {
            var folderName = "dataquality/accounttransactions/schema/invalidschema/";

            yield return
                new TestCaseData(folderName, "ReqData_MissingAmt_R_AccountTransaction_20191029_20191029095900",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.curated, new Metrics
                            {
                                adjustedBoundaryRows = 0,
                                curatedRows = 1,
                                quality = 2,
                                rejectedCSVRows = 0,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 1,
                                sourceRows = 2
                            }), new Csv(folderName + "ReqData_MissingAmt_R_AccountTransaction_curated.baseline")),
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.rejected, new Metrics
                            {
                                adjustedBoundaryRows = 0,
                                curatedRows = 1,
                                quality = 1,
                                rejectedCSVRows = 0,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 1,
                                sourceRows = 2
                            }), new Csv(folderName + "ReqData_MissingAmt_R_AccountTransaction_rejected.baseline")))
                    .SetName("Schema_ReqDataMissingAmount");


            yield return
                new TestCaseData(folderName, "ReqData_MissingIds_R_AccountTransaction_20191029_20191029095900", null,
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                                Category.AccountTransaction,
                                Storage.rejected, new Metrics
                                {
                                    adjustedBoundaryRows = 0,
                                    curatedRows = 0,
                                    quality = 1,
                                    rejectedCSVRows = 0,
                                    rejectedConstraintRows = 0,
                                    rejectedSchemaRows = 2,
                                    sourceRows = 2
                                }),
                            new Csv(folderName +
                                    "ReqData_MissingIds_R_AccountTransaction_20191029_20191029095900.csv")))
                    .SetName("Schema_MissingIds");


            yield return
                new TestCaseData(folderName, "ReqData_MissMCCCode_R_AccountTransaction_20191029_20191029095900",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.curated, new Metrics
                            {
                                adjustedBoundaryRows = 0,
                                curatedRows = 1,
                                quality = 2,
                                rejectedCSVRows = 0,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 1,
                                sourceRows = 2
                            }), new Csv(folderName + "ReqData_MissMCCCode_R_AccountTransaction_curated.baseline")),
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.rejected, new Metrics
                            {
                                adjustedBoundaryRows = 0,
                                curatedRows = 1,
                                quality = 1,
                                rejectedCSVRows = 0,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 1,
                                sourceRows = 2
                            }), new Csv(folderName + "ReqData_MissMCCCode_R_AccountTransaction_curated.baseline")))
                    .SetName("Schema_ReqDataMissingMCCCode");


            yield return
                new TestCaseData(folderName, "ReqData_MissMemoField_R_AccountTransaction_20191029_20191029095900", null,
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                                Category.AccountTransaction,
                                Storage.rejected, new Metrics
                                {
                                    adjustedBoundaryRows = 0,
                                    curatedRows = 0,
                                    quality = 1,
                                    rejectedCSVRows = 0,
                                    rejectedConstraintRows = 0,
                                    rejectedSchemaRows = 2,
                                    sourceRows = 2
                                }),
                            new Csv(folderName +
                                    "ReqData_MissMemoField_R_AccountTransaction_20191029_20191029095900.csv")))
                    .SetName("Schema_MissingMemoField");

            yield return
                new TestCaseData(folderName, "ReqData_MissingTransDate_R_AccountTransaction_20191029_20191029095900",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.curated, new Metrics
                            {
                                adjustedBoundaryRows = 0,
                                curatedRows = 1,
                                quality = 2,
                                rejectedCSVRows = 0,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 1,
                                sourceRows = 2
                            }), new Csv(folderName + "ReqData_MissingTransDate_R_AccountTransaction_curated.baseline")),
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                                Category.AccountTransaction,
                                Storage.rejected, new Metrics
                                {
                                    adjustedBoundaryRows = 0,
                                    curatedRows = 1,
                                    quality = 1,
                                    rejectedCSVRows = 0,
                                    rejectedConstraintRows = 0,
                                    rejectedSchemaRows = 1,
                                    sourceRows = 2
                                }),
                            new Csv(folderName + "ReqData_MissingTransDate_R_AccountTransaction_rejected.baseline")))
                    .SetName("Schema_ReqDataMissingTransactionDate");
            yield return
                new TestCaseData(folderName, "ReqData_MissingPostDate_R_AccountTransaction_20191029_20191029095900",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.curated, new Metrics
                            {
                                adjustedBoundaryRows = 0,
                                curatedRows = 1,
                                quality = 2,
                                rejectedCSVRows = 0,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 1,
                                sourceRows = 2
                            }), new Csv(folderName + "ReqData_MissingPostDate_R_AccountTransaction_curated.baseline")),
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.rejected, new Metrics
                            {
                                adjustedBoundaryRows = 0,
                                curatedRows = 1,
                                quality = 1,
                                rejectedCSVRows = 0,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 1,
                                sourceRows = 2
                            }), new Csv(folderName + "ReqData_MissingPostDate_R_AccountTransaction_rejected.baseline")))
                    .SetName("Schema_ReqDataMissingTransactionPostDate");
            yield return
                new TestCaseData(folderName, "ReqData_MissingTranCat_R_AccountTransaction_20191029_20191029095900",
                        null,
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                                Category.AccountTransaction,
                                Storage.rejected, new Metrics
                                {
                                    adjustedBoundaryRows = 0,
                                    curatedRows = 0,
                                    quality = 1,
                                    rejectedCSVRows = 0,
                                    rejectedConstraintRows = 0,
                                    rejectedSchemaRows = 2,
                                    sourceRows = 2
                                }),
                            new Csv(folderName +
                                    "ReqData_MissingTranCat_R_AccountTransaction_20191029_20191029095900.csv")))
                    .SetName("Schema_MissingTransactionCategory");
        }
    }
}