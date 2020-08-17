using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Insights.AccountTransactionClassifier.Function;
using Insights.AccountTransactionClassifier.Function.Azure;
using Insights.AccountTransactionClassifier.Function.Classifier;
using Insights.AccountTransactionClassifier.Function.Normalizer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NCrunch.Framework;
using Xunit;

namespace AccountTransactionClassifier.FunctionalTests
{
    [Trait("Capability", "Managed Identity")]   // NOTE: Currently, this is required via configuration.
    [Timeout(5 * 60 * 1000)]
    public class ClassifyBatchProcessTests
    {
        [Fact]
        public async Task Should_Create()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Local.json", true)
                .AddJsonFile("appsettings.Test.json")
                .Build();

            var process = new AccountTransactionClassifyBatchProcess(
                new AzureBankAccountTransactionClassifier(
                    new AccountTransactionNormalizer(),
                    new AzureMachineLearningService(new RetryPolicy())
                    {
                        BaseUrl = configuration["Components:AzureCreditsBankTransactionClassifier:Endpoint"],
                        ApiKey = configuration["Components:AzureCreditsBankTransactionClassifier:Key"]
                    },
                    new AzureMachineLearningService(new RetryPolicy())
                    {
                        BaseUrl = configuration["Components:AzureDebitsBankTransactionClassifier:Endpoint"],
                        ApiKey = configuration["Components:AzureDebitsBankTransactionClassifier:Key"]
                    }),
                new BlobServiceClient(new Uri(configuration["Services:Provisioning:PartnerEscrowStorage:ServiceUrl"])),
                NullLogger<AccountTransactionClassifyBatchProcess>.Instance);

            var now = DateTime.UtcNow;

            // Act
            await process.Run(
                "6c34c5bb-b083-4e62-a83e-cb0532754809",
                "QuarterSpot_Laso_R_AccountTransaction_v0.3_20200806_20200806140715_SingleCustomer.csv",
                $"Laso_QuarterSpot_R_AccountTransactionClass_v0.3_{now:yyyyMMdd}_{now:yyyyMMddHHmmss}.csv",
                CancellationToken.None);

            // Assert
        }
    }
}
