using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Laso.Insights.FunctionalTests.Services.DataPipeline.DataQuality.Demographic
{
    [TestFixture]
    [Parallelizable(ParallelScope.Fixtures)]
    public class DemographicDataQualityCsvTests : DataPipelineTests
    {
        [Test, Timeout(720000)]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesCsvValidation))]
        public async Task ValidDemographicCsvVariation(string folderName, string fileName,
            DataQualityParts expectedCurated, DataQualityParts expectedRejected)
        {
            await DataQualityTest(folderName, fileName, expectedCurated);
        }

        public static IEnumerable<TestCaseData> DataFilesCsvValidation()
        {
            var folderName = "dataqualityv4/demographic/csv/validcsv/";
            string csvBaseline = "ValidCsvMatch_Laso_D_Demographic_20200415_20200415.csv"; 
            
            Csv csv = new Csv(folderName+csvBaseline);
            
            yield return
                new TestCaseData(folderName,
                        "ValidCsvMatch_Laso_D_Demographic_20200415_20200415", 
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
                            Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), csv), null)
                    .SetName("ValidCsvDemographicExactMatch");
            yield return
                new TestCaseData(folderName, 
                        "ValidCsv_AllUpper_D_Demographic_20200304_20200304",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
                            Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), csv), null)
                    .SetName("ValidCsvDemographicAllUpperCase");
                    
            
            yield return
                new TestCaseData(folderName,
                        "ValidCsv_AllLower_D_Demographic_20200304_20200304",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
                            Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), csv), null)
                    .SetName("ValidCsvDemographicAllLowerCase");
            
            
            yield return
                new TestCaseData(folderName, 
                        "ValidCsv_CamelCase_D_Demographic_20200304_20200304",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
                            Storage.curated, new ExpectedMetrics().GetTestCsvAllCuratedExpectedMetrics()), csv), null)
                     .SetName("ValidCsvDemographicCamelCase");
        }

        [Test, Timeout(720000)]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesInvalidCsv))]
        public async Task InvalidDemographicCsvVariation(string folderName, string fileName,
            DataQualityParts expectedCurated, DataQualityParts expectedRejected,
            List<string> errorListInRejectedManifest)
        {

            string csvBaseline = folderName + fileName + ".csv";

            Csv csv = new Csv(csvBaseline);

            expectedRejected.expectedManifest.documents[0].errors = errorListInRejectedManifest;
            expectedRejected.Csv = csv;
            await DataQualityTest(folderName, fileName, expectedCurated, expectedRejected);
        }

        public static IEnumerable<TestCaseData> DataFilesInvalidCsv()
        {

            var folderName = "dataqualityv4/demographic/csv/invalidcsv/";
            
            yield return
                new TestCaseData(folderName, "ExtraColumnsBeg_Laso_D_Demographic_20200306_20200306", null,
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
                            Storage.rejected, new ExpectedMetrics().GetTestCsvAllRejectedExpectedMetrics(3)), null),

                        new List<string>
                        {
                            "RULE CSV.1 - Column Count: 6 5",
                            "RULE CSV.2 - Header column mismatch EXTRACOLUMN:LASO_CATEGORY",
                            "RULE CSV.2 - Header column mismatch LASO_CATEGORY:ClientKey_id",
                            "RULE CSV.2 - Header column mismatch ClientKey_id:BRANCH_ID",
                            "RULE CSV.2 - Header column mismatch BRANCH_ID:CREDIT_SCORE",
                            "RULE CSV.2 - Header column mismatch CREDIT_SCORE:CREDIT_SCORE_SOURCE",
                            "RULE CSV.2 - Header column mismatch CREDIT_SCORE_SOURCE:None"
                        })
                    .SetName("InvalidCsvExtraColumnsBeginning");
            

             
             yield return
             new TestCaseData(folderName, "ExtraColumnsEnd_Laso_D_Demographic_20200307_20200307", null,
                     new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                         Category.Demographic,
                         Storage.rejected, new ExpectedMetrics().GetTestCsvAllRejectedExpectedMetrics(3)), null),

                     new List<string>
                     {
                         "RULE CSV.1 - Column Count: 6 5",
                         "RULE CSV.2 - Header column mismatch EXTRACOLUMN:None"
                     })
                 .SetName("InvalidCsvExtraColumnsEnd");
              

                        
             yield return
             new TestCaseData(folderName, "Csv_MissReqColumn_D_Demographic_20200306_20200306", null,
                     new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                         Category.Demographic,
                         Storage.rejected, new ExpectedMetrics().GetTestCsvAllRejectedExpectedMetrics(3)), null),

                     new List<string>
                     {
                         "RULE CSV.1 - Column Count: 4 5",
                         "RULE CSV.2 - Header column mismatch BRANCH_ID:ClientKey_id",
                         "RULE CSV.2 - Header column mismatch CREDIT_SCORE:BRANCH_ID",
                         "RULE CSV.2 - Header column mismatch CREDIT_SCORE_SOURCE:CREDIT_SCORE",
                         "RULE CSV.2 - Header column mismatch None:CREDIT_SCORE_SOURCE"
                     })
                 .SetName("InvalidCsvMissRequiredColumn");

            yield return
                new TestCaseData(folderName, "WrongCategory_Laso_D_AccountTransaction_20200303_20200303", null,
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.AccountTransaction,
                            Storage.rejected, new ExpectedMetrics().GetTestCsvAllRejectedExpectedMetrics(3)), null),

                        new List<string>
                        {
                            "RULE CSV.1 - Column Count: 5 10",
                            "RULE CSV.2 - Header column mismatch ClientKey_id:AcctTranKey_id",
                            "RULE CSV.2 - Header column mismatch BRANCH_ID:ACCTKey_id",
                            "RULE CSV.2 - Header column mismatch CREDIT_SCORE:TRANSACTION_DATE",
                            "RULE CSV.2 - Header column mismatch CREDIT_SCORE_SOURCE:POST_DATE",
                            "RULE CSV.2 - Header column mismatch None:TRANSACTION_CATEGORY",
                            "RULE CSV.2 - Header column mismatch None:AMOUNT",
                            "RULE CSV.2 - Header column mismatch None:MEMO_FIELD",
                            "RULE CSV.2 - Header column mismatch None:MCC_CODE",
                            "RULE CSV.2 - Header column mismatch None:Balance_After_Transaction"                        })
                    .SetName("InvalidCsvWrongCategory");

            
            yield return
                new TestCaseData(folderName, "Csv_IncorrectOrder_D_Demographic_20200304_20200304", null, 
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
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
                            Category.Demographic,
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

            //FAILS
            //https://app.clubhouse.io/laso/story/4361/insights-dataquality-manifest-rejected-row-shown-on-2-rejection-types

            yield return
                new TestCaseData(folderName, "ExtraData_Laso_D_Demographic_20200309_20200309",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
                            Storage.curated, new Metrics
                            {
                                adjustedBoundaryRows = 0,
                                curatedRows = 3,
                                quality = 2,
                                rejectedCSVRows = 1,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 1,
                                sourceRows = 4
                            }), new Csv(folderName + "ExtraData_Laso_D_Demographic_curated.baseline")),
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
                            Storage.rejected, new Metrics
                            {
                                adjustedBoundaryRows = 0,
                                curatedRows = 3,
                                quality = 1,
                                rejectedCSVRows = 1,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 0,
                                sourceRows = 4
                            }), new Csv(folderName + "ExtraData_Laso_D_Demographic_rejected.baseline")))
                    .SetName("Csv_MalformedRow_RowExtraData");


        }
    }
}