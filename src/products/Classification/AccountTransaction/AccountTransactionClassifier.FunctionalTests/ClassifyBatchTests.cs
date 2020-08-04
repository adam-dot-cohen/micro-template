using System.Threading;
using System.Threading.Tasks;
using Insights.AccountTransactionClassifier.Function;
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

            // Act
            await ClassifyBatch.RunAsync(string.Empty, NullLogger.Instance, CancellationToken.None);

            // Assert
        }
    }
}
