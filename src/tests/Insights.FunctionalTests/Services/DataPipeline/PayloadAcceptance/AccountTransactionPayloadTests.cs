using System.Collections.Generic;
using System.Threading.Tasks;
using Laso.Insights.FunctionalTests.Services.DataPipeline.DataQuality;
using NUnit.Framework;

namespace Laso.Insights.FunctionalTests.Services.DataPipeline.PayloadAcceptance
{
    [TestFixture]
    [Parallelizable(ParallelScope.Fixtures)]
    public class AccountTransactionPayloadTests : DataPipelineTests
    {
        [Test]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesValidPayload))]
        public async Task ValidAccountTransactionPayloadVariation(string fileName, string extension = ".csv")
        {
            string folderName = "payload/accounttransactions/validpayload/";
            Manifest coldManifest = new ExpectedManifest().GetExpectedManifest(DataPipeline.Category.AccountTransaction, Storage.cold);
            Manifest rawManifest = new ExpectedManifest().GetExpectedManifest(DataPipeline.Category.AccountTransaction, Storage.raw);

            Csv expectedCsv = new Csv(folderName+fileName+".csv");

            DataQualityParts dqpCold = new DataQualityParts(coldManifest, expectedCsv);
            DataQualityParts dqpRaw = new DataQualityParts(rawManifest, expectedCsv);

            await ValidPayloadTest(folderName, fileName, dqpRaw,dqpCold);
        }

        public static IEnumerable<TestCaseData> DataFilesValidPayload()
        {
            yield return
                new TestCaseData(
                        "AllValid_Laso_D_AccountTransaction_20201029_20190427", ".csv")
                    .SetName("AccountTransactionDailyFrequency");

            yield return
                new TestCaseData(
                        "AllValid_Laso_M_AccountTransaction_20201029_20190427", ".csv")
                    .SetName("AccountTransactionMonthlyFrequency");


            yield return
                new TestCaseData(
                        "AllValid_Laso_Q_AccountTransaction_20201029_20190427", ".csv")
                    .SetName("AccountTransactionQuarterlyFrequency");


            yield return
                new TestCaseData(
                        "AllValid_Laso_R_AccountTransaction_20201029_20190427", ".csv")
                    .SetName("AccountTransactionOnRequestFrequency");

            yield return
                new TestCaseData(
                        "AllValid_Laso_W_AccountTransaction_20201029_20190427", ".csv")
                    .SetName("AccountTransactionWeeklyFrequency");

            yield return
                new TestCaseData(
                        "AllValid_Laso_Y_AccountTransaction_20201029_20190427", ".csv")
                    .SetName("AccountTransactionYearlyFrequency");

            yield return
                new TestCaseData(
                        "lowercase_frequency_w_AccountTransaction_20201029_20190427", ".csv")
                    .SetName("AccountTransactionWeeklyFrequencyLowerCase");

            //TODO PENDING IMPLEMENTATION, ONLY CSV SUPPORTED AT THIS MOMENT
            /*yield return
                new TestCaseData(
                        "Valid_AscExt_Y_AccountTransaction_20201029_20190427", ".asc")
                    .SetName("AccountTransactionAscExtension");

            yield return
                new TestCaseData(
                        "Valid_TxtExt_Y_AccountTransaction_20201029_20190427", ".txt")
                    .SetName("AccountTransactionTxtExtension");*/
        }


        [Test]
        [Ignore("TODO: Cannot validate the validation message errors in api analysis history")]
        //https://app.clubhouse.io/laso/story/4093/insights-payload-not-valid-need-to-reflect-validation-message-in-analysis-history
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesInvalidPayload))]
        public void InvalidPayloadVariationsPayloadTests(string fileName)
        {
            InvalidPayloadVariations(fileName);
        }


        public static IEnumerable<TestCaseData> DataFilesInvalidPayload()
        {
            yield return
                new TestCaseData(
                        "BadCategory_Laso_D_AccountTransactions_20201029_20190427.csv")
                    .SetName("AccountTransactionsInvalidCategory");

            yield return
                new TestCaseData(
                        "BadDate_Laso_D_AccountTransaction_20202001_20202001.csv")
                    .SetName("AccountTransactionsBadEffectiveDate");

            yield return
                new TestCaseData(
                        "InvalidNameFormat_D_AccountTransaction_20201029_20190427.csv")
                    .SetName("NotAValidFormat");

            yield return
                new TestCaseData(
                        "Unknown_Frequency_NoFreq_AccountTransaction_20201029_20190427.csv")
                    .SetName("UnknownFrequency");

            yield return
                new TestCaseData(
                        "Unknown_Extension_D_AccountTransaction_20201029_20190427.frds")
                    .SetName("UnknownExtension");
        }
    }
}