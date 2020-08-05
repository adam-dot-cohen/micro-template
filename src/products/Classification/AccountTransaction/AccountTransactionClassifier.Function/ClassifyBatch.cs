using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;
using Insights.AccountTransactionClassifier.Function.Classifier;
using Laso.Catalog.Domain.FileSchema;
using Laso.IO.Structured;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Insights.AccountTransactionClassifier.Function
{
    public class ClassifyBatch
    {
        public async Task Run(
            string partnerId,
            string filename,
            IConfiguration configuration,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var storage = GetStorage(configuration);
            var inputStream = await GetFileStream(storage, partnerId, filename, cancellationToken);

            using var reader = new DelimitedFileReader();
            reader.Open(inputStream);

            var transactions = reader
                .ReadRecords<AccountTransaction_v0_3>()
                .ToList();

            var classifier = new AzureBankAccountTransactionClassifier();
            var classes = await classifier.Classify(transactions, cancellationToken);
        }

        private static BlobServiceClient GetStorage(IConfiguration configuration)
        {
            var storageUrl = new Uri(configuration["Services:Provisioning:PartnerEscrowStorage:ServiceUrl"]);
            return new BlobServiceClient(storageUrl, new DefaultAzureCredential());
        }

        private static async Task<Stream> GetFileStream(
            BlobServiceClient storage, 
            string partnerId,
            string filename,
            CancellationToken cancellationToken)
        {
            var fileSystemName = $"transfer-{partnerId}";
            var container = storage.GetBlobContainerClient(fileSystemName);
            var blob = container.GetBlobClient(filename);
            var response = await blob.DownloadAsync(cancellationToken);

            return response.Value.Content;
        }
    }
}
