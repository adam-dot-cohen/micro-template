using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Insights.AccountTransactionClassifier.Function;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AccountTransactionClassifier.FunctionalTests
{
    [Trait("Capability", "Managed Identity")]   // NOTE: Currently, this is required via configuration.
    public class ClassifyBatchProcessTests
    {
        [Fact]
        public async Task Should_Create()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json")
                .Build();

            var job = new ClassifyBatchProcess();

            var now = DateTime.UtcNow;

            // Act

            await job.Run(
                "6c34c5bb-b083-4e62-a83e-cb0532754809", 
                "QuarterSpot_Laso_R_AccountTransaction_v0.3_20200803_20200803181652.csv",
                $"Laso_QuarterSpot_R_AccountTransactionClass_v0.3_{now:yyyyMMdd}_{now:yyyyMMddHHmmss}",
                configuration,
                NullLogger.Instance,
                CancellationToken.None);

            // Assert
        }
    }
}
