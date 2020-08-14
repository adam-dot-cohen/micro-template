using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Insights.AccountTransactionClassifier.Function;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NCrunch.Framework;
using Xunit;

namespace AccountTransactionClassifier.FunctionalTests
{
    [Trait("Capability", "Managed Identity")]   // NOTE: Currently, this is required via configuration.
    [Timeout(5 * 60 * 1000)]
    public class AzureServiceBusClassifyBatchTests
    {
        private readonly string Message =
            "{\r\n  \"specversion\": \"1.0\",\r\n  \"traceparent\": \"00-07bb1dbfd72093469c27bb7a341ef58e-e41ac550b701574d-00\",\r\n  \"type\": \"com.laso.scheduling.experimentRunScheduledEventV1\",\r\n  \"source\": \"app://services/scheduling\",\r\n  \"id\": \"beaa88b9-63b5-410a-81e7-95e5cd77bf78\",\r\n  \"time\": \"2020-08-14T04:43:06.0181078Z\",\r\n  \"data\": {\r\n    \"PartnerId\": \"6c34c5bb-b083-4e62-a83e-cb0532754809\",\r\n    \"ExperimentScheduleId\": \"cf8c937b-77a6-4d8c-8a33-ee3a516f12f3\",\r\n    \"FileBatchId\": \"bc8c6b05-31bf-43f6-929b-631642c06cfe\",\r\n    \"Files\": [\r\n      {\r\n        \"Uri\": \"https://lasodevinsightsescrow.blob.core.windows.net/transfer-6c34c5bb-b083-4e62-a83e-cb0532754809/incoming/QuarterSpot_Laso_R_AccountTransaction_v0.3_20200806_20200806140715_SingleCustomer.csv\",\r\n        \"DataCategory\": \"AccountTransaction\"\r\n      }\r\n    ]\r\n  }\r\n}";

        [Fact]
        public async Task Should_Process()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Local.json", true)
                .AddJsonFile("appsettings.Test.json")
                .Build();

            var trigger = new AzureServiceBusClassifyBatch(
                configuration, NullLogger<AzureServiceBusClassifyBatch>.Instance);

            // Act
            await trigger.Run(Message, CancellationToken.None);

        }
    }
}
