using System.Threading;
using System.Threading.Tasks;
using Insights.AccountTransactionClassifier.Function;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AccountTransactionClassifier.FunctionalTests
{
    public class ClassifyBatchTests
    {
        [Fact]
        public async Task Should_Create()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().Build();
            var job = new ClassifyBatch();

            // Act
            await job.Run(
                "6c34c5bb-b083-4e62-a83e-cb0532754809", 
                "incoming/QuarterSpot_Laso_R_AccountTransaction_v0.3_20200803_20200803181652.csv",
                configuration,
                NullLogger.Instance,
                CancellationToken.None);

            // Assert
        }
    }
}
