
using System.Collections.Generic;
using NUnit.Framework;

namespace Laso.Insights.FunctionalTests.Services.DataPipeline.DataQuality
{

    [TestFixture()]
    [Parallelizable(ParallelScope.Fixtures)]

    public class DemographicDataQualityTests : GlobalSetUp
    {


        public static IEnumerable<TestCaseData> DataFilesSchemaValidation()
        {
            yield return
                new TestCaseData(
                        "")
                    .SetName("ValidSchema_Laso_D_Demographic_20200415_20200415.csv");
          /*  yield return
                new TestCaseData(
                        "")
                    .SetName("ValidSchemaMissingOptionalColumns");*/
            yield return
                new TestCaseData("")
                    .SetName("ValidSchema_AllUpper_D_Demographic_20200304_20200304.csv");
            yield return
                new TestCaseData("")
                    .SetName("ValidSchema_AllLower_D_Demographic_20200304_20200304.csv");
        }

        public static IEnumerable<TestCaseData> DataFilesInvalidSchema()
        {
            yield return
                new TestCaseData("")
                    .SetName("ExtraColumnsBeg_Laso_D_Demographic_202003008_20200308.csv");
            yield return
                new TestCaseData("")
                    .SetName("ExtraColumnsEnd_Laso_D_Demographic_202003008_20200308.csv");
            yield return
                new TestCaseData("")
                    .SetName("ExtraColumnsMiddle_Laso_D_Demographic_202003008_20200308.csv");

            yield return
                new TestCaseData("")
                    .SetName("NoHeader_Laso_D_Demographic_20200310_20200310.csv");
            yield return
                new TestCaseData("")
                    .SetName("Schema_IncorrectOrder_D_20200304_20200304.csv");


            yield return
                new TestCaseData(
                        "WrongCategory_Laso_D_AccountTransaction_20200303_20200303.csv")
                    .SetName("InvalidSchemaMixedCategory");

            yield return
                new TestCaseData("")
                    .SetName("InvalidSchema_MissRequiredColumn_D_Demographic_202003006_20200306.csv");
    

        }



        public void DemographicCategoryValidExplicitSchemaShouldPass()
        {
        }

        public void DemographicCategoryInferredSchemaShouldPass()
        {
        }

        public void DemographicCategoryInvalidSchema()
        {
        }

        public void DemographicCategoryDataQualityAllGoodDataPipelineAllCurated()
        {
        }
        public void DemographicCategoryDataQualityNumberOfRows()
        {
        }

        public void DemographicCategoryDataQualityPipelineAllBadDataAllRejected()
        {
        }
        public void DemographicCategoryDataQualityPipelinePartialGoodDataRejectedCurated()
        {
        }

        public void DemographicCategoryDataQualityPipelineRowsMissingRequiredDataInRejected()
        {
        }

        public void DemographicCategoryDataQualityPipelineRowsMissingOptionalDataInCurated()
        {
        }

        public void DemographicCategoryDataQualityPipelineRowsExtraColumns()
        {
        }

    }

}
