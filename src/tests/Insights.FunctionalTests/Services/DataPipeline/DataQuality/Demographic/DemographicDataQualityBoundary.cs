using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Laso.Insights.FunctionalTests.Services.DataPipeline.DataQuality.Demographic
{
    [TestFixture]
    [Parallelizable(ParallelScope.Fixtures)]
    public class DemographicDataQualityBoundary : DataPipelineTests
    {
        
        [Test, Timeout(720000)]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(DataFilesInvalidCsv))]
        public async Task DemographicBoundaryValidation(string folderName, string fileName,
            DataQualityParts expectedCurated, DataQualityParts expectedRejected,
            List<string> errorListInRejectedManifest)
        {
            if (expectedRejected != null)
            {
                expectedRejected.expectedManifest.documents[0].errors = errorListInRejectedManifest;

                if (expectedRejected.Csv == null)
                {
                    string csvBaseline = folderName + fileName + ".csv";
                    Csv csv = new Csv(csvBaseline);
                    expectedRejected.Csv = csv;
                }
            }

            await DataQualityTest(folderName, fileName, expectedCurated, expectedRejected);
        }

        public static IEnumerable<TestCaseData> DataFilesInvalidCsv()
        {

            var folderName = "dataqualityv4/demographic/boundary/";
        
            yield return
                new TestCaseData(folderName,
                        "D_Boundary_D_Demographic_20200309_20200309",
                        new DataQualityParts(new ExpectedManifest().GetExpectedManifest(
                            Category.Demographic,
                            Storage.curated, new Metrics
                            {
                                adjustedBoundaryRows = 4,
                                curatedRows = 5,
                                quality = 3,
                                rejectedCSVRows = 0,
                                rejectedConstraintRows = 0,
                                rejectedSchemaRows = 0,
                                sourceRows = 5
                            }), new Csv(folderName + "D_Boundary_D_Demographic_curated.baseline")), null,null)
                    .SetName("DemographicBoundaryTestCreditScore");

        }
    }
}