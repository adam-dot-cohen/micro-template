using System;
using NUnit.Framework;

namespace Laso.Insights.FunctionalTests.Services.DataPipeline.DataQuality
{

    [TestFixture()]
    [Parallelizable(ParallelScope.Fixtures)]

    public class DataQualityTests : GlobalSetUp
    {

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
