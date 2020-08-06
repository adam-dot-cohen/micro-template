using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Insights.AccountTransactionClassifier.Function.Azure;
using Laso.Catalog.Domain.FileSchema;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

namespace AccountTransactionClassifier.FunctionalTests
{
    [Trait("Capability", "Managed Identity")]   // NOTE: Currently, this is required via configuration.
    public class AzureMachineLearningServiceTests
    {
        [Fact]
        public async Task Should_Get_Schema()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Local.json", true)
                .AddJsonFile("appsettings.Test.json")
                .Build();

            var service = new AzureMachineLearningService(new RetryPolicy())
            {
                BaseUrl = configuration["AzureCreditsBankTransactionClassifierApiEndpoint"],
                ApiKey = configuration["AzureCreditsBankTransactionClassifierApiKey"]
            };

            // Act
            var schema = await service.GetSchema(CancellationToken.None);

            // Assert
            schema.Inputs.Length.ShouldBe(1);
            schema.Outputs.Length.ShouldBe(1);
        }

        [Fact]
        public async Task Should_Execute()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Local.json", true)
                .AddJsonFile("appsettings.Test.json")
                .Build();

            var service = new AzureMachineLearningService(new RetryPolicy())
            {
                BaseUrl = configuration["AzureCreditsBankTransactionClassifierApiEndpoint"],
                ApiKey = configuration["AzureCreditsBankTransactionClassifierApiKey"]
            };

            var transactions = new List<AccountTransaction_v0_3>
            {
                new AccountTransaction_v0_3 { Memo_Field = "family dollar family dollar ha columbus oh5347401890860340" },
                new AccountTransaction_v0_3 { Memo_Field = "01 online banking xfr to" }
            };

            var inputs = transactions
                .Select(t => new MachineLearningExecutionObject
                {
                    ["input1"] = new Dictionary<string, object?> { ["NormalizedText"] = t.Memo_Field }
                });

            // Act
            var response = await service.Execute(inputs, CancellationToken.None);
            var result = response.ToList();

            // Assert
            result.Count.ShouldBe(2);
            result[0]["output1"]["Scored Labels"].ShouldBe(6d);
            result[1]["output1"]["Scored Labels"].ShouldBe(47d);
        }
    }
}
