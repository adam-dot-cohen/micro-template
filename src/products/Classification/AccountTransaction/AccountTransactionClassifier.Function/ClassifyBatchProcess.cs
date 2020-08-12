using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Insights.AccountTransactionClassifier.Function.Azure;
using Insights.AccountTransactionClassifier.Function.Classifier;
using Insights.AccountTransactionClassifier.Function.Normalizer;
using Laso.Catalog.Domain.FileSchema.Input;
using Laso.IO.Structured;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Insights.AccountTransactionClassifier.Function
{
    public class ClassifyBatchProcess
    {
        //public async Task Run(
        //    string partnerId,
        //    string inputFilename,
        //    string outputFilename,
        //    IConfiguration configuration,
        //    ILogger logger,
        //    CancellationToken cancellationToken)
        //{
        //    logger.LogDebug("Opening input blob file.");
        //    var storage = GetStorage(configuration);
        //    var inputStream = await BlobOpenRead(storage, partnerId, inputFilename, cancellationToken);

        //    logger.LogDebug("Creating file reader.");
        //    using var reader = new DelimitedFileReader();
        //    reader.Open(inputStream);

        //    logger.LogDebug("Reading transactions.");
        //    var transactions = reader
        //        .ReadRecords<AccountTransaction_v0_3>()
        //        .ToList();

        //    logger.LogDebug("Classifying transactions.");
        //    var normalizer = new AccountTransactionNormalizer();
        //    var creditsMachineLearningService = new AzureMachineLearningService(new RetryPolicy())
        //    {
        //        BaseUrl = configuration["AzureCreditsBankTransactionClassifierApiEndpoint"],
        //        ApiKey = configuration["AzureCreditsBankTransactionClassifierApiKey"]
        //    };
        //    var debitsMachineLearningService = new AzureMachineLearningService(new RetryPolicy())
        //    {
        //        BaseUrl = configuration["AzureDebitsBankTransactionClassifierApiEndpoint"],
        //        ApiKey = configuration["AzureDebitsBankTransactionClassifierApiKey"]
        //    };
        //    var classifier = new AzureBankAccountTransactionClassifier(normalizer, creditsMachineLearningService, debitsMachineLearningService);
        //    var classes = await classifier.Classify(transactions, cancellationToken);

        //    logger.LogDebug("Writing classes.");
        //    using var writer = new DelimitedFileWriter();
        //    await using var outputStream = new MemoryStream();
        //    writer.Open(outputStream);
        //    writer.WriteRecords(classes);
        //    outputStream.Seek(0, SeekOrigin.Begin);

        //    logger.LogDebug("Saving output blob file.");
        //    await BlobWrite(storage, partnerId, outputFilename, outputStream, cancellationToken);
        //}

        //private static BlobServiceClient GetStorage(IConfiguration configuration)
        //{
        //    var storageUrl = new Uri(configuration["Services:Provisioning:PartnerEscrowStorage:ServiceUrl"]);
        //    return new BlobServiceClient(storageUrl, new DefaultAzureCredential());
        //}

        //private static async Task<Stream> BlobOpenRead(
        //    BlobServiceClient storage, 
        //    string partnerId,
        //    string filename,
        //    CancellationToken cancellationToken)
        //{
        //    var fileSystemName = $"transfer-{partnerId}";
        //    var container = storage.GetBlobContainerClient(fileSystemName);
        //    var inputFilename = $"incoming/{filename}";
        //    var blob = container.GetBlobClient(inputFilename);

        //    // TODO: Needs - "Storage Blob Data Reader" role in Azure IAM
        //    var response = await blob.DownloadAsync(cancellationToken);

        //    return response.Value.Content;
        //}

        //private static async Task<string> BlobWrite(
        //    BlobServiceClient storage, 
        //    string partnerId,
        //    string filename,
        //    Stream outputStream,
        //    CancellationToken cancellationToken)
        //{
        //    var fileSystemName = $"transfer-{partnerId}";
        //    var container = storage.GetBlobContainerClient(fileSystemName);
        //    var outputFilename = $"outgoing/{filename}";
        //    var blob = container.GetBlobClient(outputFilename);

        //    // TODO: Needs - "Storage Blob Data Contributor" role in Azure IAM
        //    var response = await blob.UploadAsync(outputStream, cancellationToken);

        //    return response.Value.ETag.ToString();
        //}
    }
}
