using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Laso.Insights.FunctionalTests.Services.DataPipeline.PayloadAcceptance
{
    [TestFixture]
    [Parallelizable(ParallelScope.Fixtures)]
    public class DemographicPayloadTests : DataPipelineTests
    {
 
        [Test]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesValidPayload))]
        public async Task ValidDemographicCsvPayloadVariation(string fileName)
        {
            string folderName = "payload/demographic/validpayload/";
            Manifest coldManifest = GetExpectedManifest(DataPipeline.Category.Demographic, Storage.cold);
            Manifest rawManifest = GetExpectedManifest(DataPipeline.Category.Demographic, Storage.raw);
            Csv expectedCsv = new Csv(folderName+fileName+".csv");
            List <Manifest> manifestsExpected = new List<Manifest> {coldManifest, rawManifest};
            await ValidPayloadTest(folderName,fileName,manifestsExpected,expectedCsv);
        }


        public static IEnumerable<TestCaseData> DataFilesValidPayload()
        {
            yield return
                new TestCaseData(
                        "AllValidCsv_Laso_D_Demographic_20200415_20200415")
                    .SetName("DemographicDataDailyFrequency");
            
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
            yield return
                new TestCaseData("UTF8_Laso_Y_Demographic_20200415_20200415")
                    .SetName("DemographicDataUTF8Encoding");
            yield return
                new TestCaseData("UTF8Bom_Laso_Y_Demographic_20200415_20200415")
                    .SetName("DemographicDataUTF8BomEncoding");
                    
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
        public void InvalidPayloadVariationsDemographic(string fileName)
        {
            InvalidPayloadVariations(fileName);
        }


        public static IEnumerable<TestCaseData> DataFilesInvalidPayload()
        {
            yield return
                new TestCaseData(
                        "Frequency_Laso_Invalid_Demographic_20200415_20200415.csv")
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
                    .SetName("UnknownExtension");

            //TODO: Need to understand the limit in name length, go past the limit
            //https://app.clubhouse.io/laso/story/4045/data-processing-error-when-file-name-is-too-long
            yield return
                new TestCaseData(
                        "asdfasfsafsfdsafsaf_asdfsafasfabtgryrtytertafasf_D_Demographic_202003006_20200306.csv")
                    .SetName("PayloadFileNameExceedsLengthLimit");
        }


 
    }
}