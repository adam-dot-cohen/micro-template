using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Laso.Insights.FunctionalTests.Services.DataPipeline.DataQuality.AccountTransactions
{
    [Parallelizable(ParallelScope.Fixtures)]
    public class AccountTransactionsDataQualityCsvTests : DataPipelineTests
    {
        [Test, Timeout(720000)]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesCsvValidation))]
        public async Task ValidAccountTransactionsCsvVariation(string folderName, string fileName,
            DataQualityParts expectedCurated, DataQualityParts expectedRejected)
        {
            await DataQualityTest(folderName, fileName, expectedCurated);
        }

        public static IEnumerable<TestCaseData> DataFilesCsvValidation()
        {
            var folderName = "dataqualityv4/accounttransactions/csv/validcsv/";
            var csvBaseline = "ValidCsvMatch_Laso_R_AccountTransaction_20191029_20191029095900.csv";

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


        [Test, Timeout(720000)]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesInvalidCsv))]
        public async Task InvalidAccountTransactionsCsvVariation(string folderName, string fileName,
            DataQualityParts expectedCurated, DataQualityParts expectedRejected,
            List<string> errorListInRejectedManifest)
        {
            var csvBaseline = folderName + fileName + ".csv";

            var csv = new Csv(csvBaseline);

            expectedRejected.expectedManifest.documents[0].errors = errorListInRejectedManifest;
            expectedRejected.Csv = csv;
            await DataQualityTest(folderName, fileName, expectedCurated, expectedRejected);
        }

        public static IEnumerable<TestCaseData> DataFilesInvalidCsv()
        {
 
            var folderName = "dataqualityv4/accounttransactions/csv/invalidcsv/";
            
            yield return
                new TestCaseData(folderName, "Csv_ExtraColumnMiddle_R_AccountTransaction_20191029_20191029095900", null,
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.rejected, new ExpectedMetrics().GetTestCsvAllRejectedExpectedMetrics(3)), null),
                        new List<string>
                        {
                            "RULE CSV.1 - Column Count: 11 10",
                            "RULE CSV.2 - Header column mismatch Notes:TRANSACTION_DATE",
                            "RULE CSV.2 - Header column mismatch TRANSACTION_DATE:POST_DATE",
                            "RULE CSV.2 - Header column mismatch POST_DATE:TRANSACTION_CATEGORY",
                            "RULE CSV.2 - Header column mismatch TRANSACTION_CATEGORY:AMOUNT",
                            "RULE CSV.2 - Header column mismatch AMOUNT:MEMO_FIELD",
                            "RULE CSV.2 - Header column mismatch MEMO_FIELD:MCC_CODE",
                            "RULE CSV.2 - Header column mismatch MCC_CODE:Balance_After_Transaction",
                            "RULE CSV.2 - Header column mismatch Balance_After_Transaction:None"                        })
                    .SetName("ExtraColumnInTheMiddle");
               

            
            yield return
                new TestCaseData(folderName, "Csv_ExtraColumnBeg_R_AccountTransaction_20191029_20191029095900", null,
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.rejected, new ExpectedMetrics().GetTestCsvAllRejectedExpectedMetrics(3)), null),
                        new List<string>
                        {
                            "RULE CSV.1 - Column Count: 11 10",
                            "RULE CSV.2 - Header column mismatch Notes:LASO_CATEGORY",
                            "RULE CSV.2 - Header column mismatch LASO_CATEGORY:AcctTranKey_id",
                            "RULE CSV.2 - Header column mismatch AcctTranKey_id:ACCTKey_id",
                            "RULE CSV.2 - Header column mismatch ACCTKey_id:TRANSACTION_DATE",
                            "RULE CSV.2 - Header column mismatch TRANSACTION_DATE:POST_DATE",
                            "RULE CSV.2 - Header column mismatch POST_DATE:TRANSACTION_CATEGORY",
                            "RULE CSV.2 - Header column mismatch TRANSACTION_CATEGORY:AMOUNT",
                            "RULE CSV.2 - Header column mismatch AMOUNT:MEMO_FIELD",
                            "RULE CSV.2 - Header column mismatch MEMO_FIELD:MCC_CODE",
                            "RULE CSV.2 - Header column mismatch MCC_CODE:Balance_After_Transaction",
                            "RULE CSV.2 - Header column mismatch Balance_After_Transaction:None"                        })
                    .SetName("InvalidCsvExtraColumnsBeginning");
            yield return
                new TestCaseData(folderName, "Csv_ExtraColumnEnd_R_AccountTransaction_20191029_20191029095900", null,
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.rejected, new ExpectedMetrics().GetTestCsvAllRejectedExpectedMetrics(3)), null),
                        new List<string>
                        {
                            "RULE CSV.1 - Column Count: 11 10",
                            "RULE CSV.2 - Header column mismatch Notes:None"                        })
                    .SetName("InvalidCsvExtraColumnsEnd");
                                yield return
                new TestCaseData(folderName, "Csv_MissingReqHeader_R_AccountTransaction_20191029_20191029095900", null,
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.rejected, new ExpectedMetrics().GetTestCsvAllRejectedExpectedMetrics(4)), null),
                        new List<string>
                        {
                            "RULE CSV.1 - Column Count: 8 10",
                            "RULE CSV.2 - Header column mismatch MEMO_FIELD:AMOUNT",
                            "RULE CSV.2 - Header column mismatch MCC_CODE:MEMO_FIELD",
                            "RULE CSV.2 - Header column mismatch None:MCC_CODE",
                            "RULE CSV.2 - Header column mismatch None:Balance_After_Transaction"                        })
                    .SetName("MissingRequiredHeader");
            yield return
                new TestCaseData(folderName, "Csv_WrongCategory_R_Demographic_20191029_20191029095900", null,
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
                            Storage.rejected, new ExpectedMetrics().GetTestCsvAllRejectedExpectedMetrics(3)), null),
                        new List<string>
                        {
                            "RULE CSV.1 - Column Count: 10 5",
                            "RULE CSV.2 - Header column mismatch AcctTranKey_id:ClientKey_id",
                            "RULE CSV.2 - Header column mismatch ACCTKey_id:BRANCH_ID",
                            "RULE CSV.2 - Header column mismatch TRANSACTION_DATE:CREDIT_SCORE",
                            "RULE CSV.2 - Header column mismatch POST_DATE:CREDIT_SCORE_SOURCE",
                            "RULE CSV.2 - Header column mismatch TRANSACTION_CATEGORY:None",
                            "RULE CSV.2 - Header column mismatch AMOUNT:None",
                            "RULE CSV.2 - Header column mismatch MEMO_FIELD:None",
                            "RULE CSV.2 - Header column mismatch MCC_CODE:None",
                            "RULE CSV.2 - Header column mismatch Balance_After_Transaction:None"
                        })
                    .SetName("InvalidCsvWrongCategory");

            
            yield return
                new TestCaseData(folderName, "Csv_IncorrectOrder_R_AccountTransaction_20191029_20191029095900", null,
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.rejected, new ExpectedMetrics().GetTestCsvAllRejectedExpectedMetrics(3)), null),
                        new List<string>
                        {
                            "RULE CSV.2 - Header column mismatch Balance_After_Transaction:MCC_CODE",
                            "RULE CSV.2 - Header column mismatch MCC_CODE:Balance_After_Transaction"
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
                            "RULE CSV.2 - Header column mismatch ABC:MEMO_FIELD",
                            "RULE CSV.2 - Header column mismatch 4800:MCC_CODE",
                            "RULE CSV.2 - Header column mismatch 1800:Balance_After_Transaction"
                        })
                   .SetName("InvalidCsvNoHeader");
        }
    }
}