using System.Collections.Generic;
using System.Threading.Tasks;
using Laso.Insights.FunctionalTests.Utils;
using NUnit.Framework;

namespace Laso.Insights.FunctionalTests.Services.DataPipeline.AccountTransactions
{
    public class AccountTransactionsDataQualityCsvTests : DataPipelineTests
    {
        [Test]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesCsvValidation))]
        public async Task ValidAccountTransactionsCsvVariation(string folderName, string fileName,
            DataQualityParts expectedCurated, DataQualityParts expectedRejected)
        {
            await DataQualityTest(folderName, fileName, expectedCurated);
        }

        public static IEnumerable<TestCaseData> DataFilesCsvValidation()
        {
            var folderName = "dataquality/accounttransactions/csv/validcsv/";
            var csvBaseline = "ValidCsvMatch_Laso_R_AccountTransaction_20191029_20191029095900.csv";
            var expectedRows = new AzureBlobStgFactory().Create()
                .DownloadCsvFileFromAutomationStorage(folderName +
                                                      csvBaseline)
                .Result;
            var csv = new Csv(folderName+csvBaseline);

            yield return
                new TestCaseData(folderName,
                        "ValidCsvMatch_Laso_R_AccountTransaction_20191029_20191029095900",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), csv), null)
                    .SetName("ValidCsvAccountTransactionsExactMatch");
            yield return
                new TestCaseData(folderName,
                        "ValidCsvMatch_AllLower_R_AccountTransaction_20191029_20191029095900",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), csv), null)
                    .SetName("ValidCsvAccountTransactionsAllLowerCase");
            yield return
                new TestCaseData(folderName,
                        "ValidCsvMatch_AllUpper_R_AccountTransaction_20191029_20191029095900",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), csv), null)
                    .SetName("ValidCsvAccountTransactionsAllUpperCase");
            yield return
                new TestCaseData(folderName,
                        "ValidCsvMatch_CamelCase_R_AccountTransaction_20191029_20191029095900",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), csv), null)
                    .SetName("ValidCsvAccountTransactionsCamelCase");
        }


        [Test]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesInvalidCsv))]
        public async Task InvalidAccountTransactionsCsvVariation(string folderName, string fileName,
            DataQualityParts expectedCurated, DataQualityParts expectedRejected,
            List<string> errorListInRejectedManifest)
        {
            var csvBaseline = folderName + fileName + ".csv";

            var expectedRows =
                new AzureBlobStgFactory().Create()
                    .DownloadCsvFileFromAutomationStorage(folderName + fileName + ".csv").Result;

            var csv = new Csv(csvBaseline);

            expectedRejected.expectedManifest.documents[0].errors = errorListInRejectedManifest;
            expectedRejected.Csv = csv;
            await DataQualityTest(folderName, fileName, expectedCurated, expectedRejected);
        }

        public static IEnumerable<TestCaseData> DataFilesInvalidCsv()
        {
            // FOLLOWING 6 TEST CASES:
            //https://app.clubhouse.io/laso/story/4174/insights-data-quality-no-rejected-files-created-in-rejected-file-system

            var folderName = "dataquality/accounttransactions/csv/invalidcsv/";
            /*
            yield return
                new TestCaseData(folderName, "Csv_ExtraColumnMiddle_R_AccountTransaction_20191029_20191029095900", null,
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.rejected, new ExpectedMetrics().GetTestCsvAllRejectedExpectedMetrics(3)), null),
                        new List<string>
                        {
                        //replace rules
                            "RULE CSV.2 - Header column mismatch AcctTranKey_id:LASO_CATEGORY",
                            "RULE CSV.2 - Header column mismatch ACCTKey_id:AcctTranKey_id",
                            "RULE CSV.2 - Header column mismatch LASO_CATEGORY:ACCTKey_id",
                        })
                    .SetName("ExtraColumnInTheMiddle");*/


            /*
            yield return
                new TestCaseData(folderName, "Csv_ExtraColumnBeg_R_AccountTransaction_20191029_20191029095900", null,
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.rejected, new ExpectedMetrics().GetTestCsvAllRejectedExpectedMetrics(3)), null),
                        //replace rules
                        new List<string>
                        {
                            "RULE CSV.2 - Header column mismatch AcctTranKey_id:LASO_CATEGORY",
                            "RULE CSV.2 - Header column mismatch ACCTKey_id:AcctTranKey_id",
                            "RULE CSV.2 - Header column mismatch LASO_CATEGORY:ACCTKey_id"
                        })
                    .SetName("InvalidCsvExtraColumnsBeginning");*/
            /*yield return
                new TestCaseData(folderName, "Csv_ExtraColumnEnd_R_AccountTransaction_20191029_20191029095900", null,
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.rejected, new ExpectedMetrics().GetTestCsvAllRejectedExpectedMetrics(3)), null),
                        //replace rules
                        new List<string>
                        {
                            "RULE CSV.2 - Header column mismatch AcctTranKey_id:LASO_CATEGORY",
                            "RULE CSV.2 - Header column mismatch ACCTKey_id:AcctTranKey_id",
                            "RULE CSV.2 - Header column mismatch LASO_CATEGORY:ACCTKey_id"
                        })
                    .SetName("InvalidCsvExtraColumnsEnd");*/
            /*yield return
                new TestCaseData(folderName, "Csv_MissingOptHeader_R_AccountTransaction_20191029_20191029095900", null,
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.rejected, new ExpectedMetrics().GetTestCsvAllRejectedExpectedMetrics(3)), null),
                        //replace rules
                        new List<string>
                        {
                            "RULE CSV.2 - Header column mismatch AcctTranKey_id:LASO_CATEGORY",
                            "RULE CSV.2 - Header column mismatch ACCTKey_id:AcctTranKey_id",
                            "RULE CSV.2 - Header column mismatch LASO_CATEGORY:ACCTKey_id"
                        })
                    .SetName("MissingOptionalHeader");
                    */
            /*yield return
                new TestCaseData(folderName, "Csv_MissingReqHeader_R_AccountTransaction_20191029_20191029095900", null,
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.rejected, new ExpectedMetrics().GetTestCsvAllRejectedExpectedMetrics(3)), null),
                        //replace rules
                        new List<string>
                        {
                            "RULE CSV.2 - Header column mismatch AcctTranKey_id:LASO_CATEGORY",
                            "RULE CSV.2 - Header column mismatch ACCTKey_id:AcctTranKey_id",
                            "RULE CSV.2 - Header column mismatch LASO_CATEGORY:ACCTKey_id"
                        })
                    .SetName("MissingRequiredHeader");
                    */
            /*yield return
                new TestCaseData(folderName, "Csv_WrongCategory_R_Demographic_20191029_20191029095900", null,
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.rejected, new ExpectedMetrics().GetTestCsvAllRejectedExpectedMetrics(3)), null),
                        //replace rules
                        new List<string>
                        {
                            "RULE CSV.2 - Header column mismatch AcctTranKey_id:LASO_CATEGORY",
                            "RULE CSV.2 - Header column mismatch ACCTKey_id:AcctTranKey_id",
                            "RULE CSV.2 - Header column mismatch LASO_CATEGORY:ACCTKey_id"
                        })
                    .SetName("InvalidCsvWrongCategory");

            */

            yield return
                new TestCaseData(folderName, "Csv_IncorrectOrder_R_AccountTransaction_20191029_20191029095900", null,
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.rejected, new ExpectedMetrics().GetTestCsvAllRejectedExpectedMetrics(3)), null),
                        new List<string>
                        {
                            "RULE CSV.2 - Header column mismatch AcctTranKey_id:LASO_CATEGORY",
                            "RULE CSV.2 - Header column mismatch ACCTKey_id:AcctTranKey_id",
                            "RULE CSV.2 - Header column mismatch LASO_CATEGORY:ACCTKey_id"
                        })
                    .SetName("InvalidCsvIncorrectOrder");

            yield return
                new TestCaseData(folderName, "Csv_MissingHeaders_R_AccountTransaction_20191029_20191029095900", null,
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.rejected, new ExpectedMetrics().GetTestCsvAllRejectedExpectedMetrics()), null),
                        new List<string>
                        {
                            "RULE CSV.2 - Header column mismatch Account_Transaction:LASO_CATEGORY",
                            "RULE CSV.2 - Header column mismatch 102:AcctTranKey_id",
                            "RULE CSV.2 - Header column mismatch 111:ACCTKey_id",
                            "RULE CSV.2 - Header column mismatch 2020-03-07:TRANSACTION_DATE",
                            "RULE CSV.2 - Header column mismatch 2020-03-07:POST_DATE",
                            "RULE CSV.2 - Header column mismatch 475 CHECK:TRANSACTION_CATEGORY",
                            "RULE CSV.2 - Header column mismatch 2100:AMOUNT",
                            "RULE CSV.2 - Header column mismatch :MEMO_FIELD",
                            "RULE CSV.2 - Header column mismatch :MCC_CODE"
                        })
                    .SetName("InvalidCsvNoHeader");
        }
    }
}