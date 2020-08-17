using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Insights.AccountTransactionClassifier.Function.Classifier;
using Laso.Catalog.Domain.FileSchema.Input;
using Laso.IO.Structured;
using Microsoft.Extensions.Logging;

namespace Insights.AccountTransactionClassifier.Function
{
    public class AccountTransactionClassifyBatchProcess : IAccountTransactionClassifyBatchProcess
    {
        private readonly IAccountTransactionClassifier _classifier;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<AccountTransactionClassifyBatchProcess> _logger;

        public AccountTransactionClassifyBatchProcess(
            IAccountTransactionClassifier classifier,
            BlobServiceClient blobServiceClient,
            ILogger<AccountTransactionClassifyBatchProcess> logger)
        {
            _classifier = classifier;
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        public async Task Run(
            string partnerId,
            string inputFilename,
            string outputFilename,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("Opening input blob file.");
            var inputStream = await BlobOpenRead(partnerId, inputFilename, cancellationToken);

            _logger.LogDebug("Creating file reader.");
            using var reader = new DelimitedFileReader();
            reader.Open(inputStream);

            _logger.LogDebug("Reading transactions.");
            var transactions = reader
                .ReadRecords<AccountTransaction_v0_3>()
                .ToList();

            _logger.LogDebug("Classifying transactions.");
            var classes = await _classifier.Classify(transactions, cancellationToken);

            _logger.LogDebug("Writing classes.");
            using var writer = new DelimitedFileWriter();
            await using var outputStream = new MemoryStream();
            writer.Open(outputStream);
            writer.WriteRecords(classes);
            outputStream.Seek(0, SeekOrigin.Begin);

            _logger.LogDebug("Saving output blob file.");
            await BlobWrite(partnerId, outputFilename, outputStream, cancellationToken);
        }

        private async Task<Stream> BlobOpenRead(
            string partnerId,
            string filename,
            CancellationToken cancellationToken)
        {
            var fileSystemName = $"transfer-{partnerId}";
            var container = _blobServiceClient.GetBlobContainerClient(fileSystemName);
            var inputFilename = $"incoming/{filename}";
            var blob = container.GetBlobClient(inputFilename);

            // TODO: Needs - "Storage Blob Data Reader" role in Azure IAM
            var response = await blob.DownloadAsync(cancellationToken);

            return response.Value.Content;
        }

        private async Task<string> BlobWrite(
            string partnerId,
            string filename,
            Stream outputStream,
            CancellationToken cancellationToken)
        {
            var fileSystemName = $"transfer-{partnerId}";
            var container = _blobServiceClient.GetBlobContainerClient(fileSystemName);
            var outputFilename = $"outgoing/{filename}";
            var blob = container.GetBlobClient(outputFilename);

            // TODO: Needs - "Storage Blob Data Contributor" role in Azure IAM
            var response = await blob.UploadAsync(outputStream, cancellationToken);

            return response.Value.ETag.ToString();
        }
    }
}
