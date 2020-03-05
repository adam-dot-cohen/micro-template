using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Laso.DataImport.Api;
using Laso.DataImport.Core.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

// todo: This should be completely async. BeginImport should return as soon as the operation
// begins, but currently it does not (didn't have messaging/events at the time.)
namespace DataImport.Jobs
{
    public static class ProcessImportSubscriptions
    {
        [FunctionName(nameof(ProcessImportSubscriptions))]
        public static async void Run([TimerTrigger("0 00 21 * * 1-5")]TimerInfo myTimer,
            Importer.ImporterClient importsClient,
            ILogger log)
        {
            var reply = await importsClient.GetAllImportSubscriptionsAsync(new GetAllImportSubscriptionsRequest());

            foreach (var subscription in reply.Subscriptions)
            {
                if (!ShouldRun(subscription))
                    continue;

                var request = new BeginImportRequest
                {
                    PartnerId = subscription.PartnerId,
                    Encryption = subscription.EncryptionType,
                    OutputContainerName = subscription.IncomingStorageLocation,
                    OutputFilePath = subscription.IncomingFilePath,
                    UpdatedAfter = subscription.LastSuccessfulImport
                };

                request.Imports.AddRange(subscription.Imports);

                var result = await importsClient.BeginImportAsync(request);

                var historyRequest = CreateImportHistoryAsync(result, subscription);
                if (result.Success)
                {
                    subscription.LastSuccessfulImport = historyRequest.History.Completed;
                    subscription.NextScheduledImport = Timestamp.FromDateTime(CalcNextImportDate(subscription));
                }

                await importsClient.CreateImportHistoryAsync(historyRequest);
                await importsClient.UpdateImportSubscriptionAsync(new UpdateImportSubscriptionRequest { Subscription = subscription });
            }
        }

        private static DateTime CalcNextImportDate(ImportSubscriptionModel subscription)
        {
            if (subscription.LastSuccessfulImport == null)
                return DateTime.UtcNow;

            var last = subscription.LastSuccessfulImport.ToDateTime();

            return subscription.Frequency switch
            {
                ImportFrequency.Daily => last.AddDays(1),
                ImportFrequency.Weekly => last.AddDays(7),
                ImportFrequency.Monthly => last.AddMonths(1),
                ImportFrequency.Quarterly => last.AddDays(365.0 / 4),
                ImportFrequency.Yearly => last.AddYears(1),
                ImportFrequency.OnRequest => DateTime.MaxValue,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static bool ShouldRun(ImportSubscriptionModel subscription)
        {
            return subscription.NextScheduledImport == null
                   || subscription.Frequency != ImportFrequency.OnRequest
                   || subscription.NextScheduledImport.ToDateTime() >= DateTime.UtcNow;
        }

        private static CreateImportHistoryRequest CreateImportHistoryAsync(BeginImportReply response, ImportSubscriptionModel subscription)
        {
            var historyRequest = new CreateImportHistoryRequest
            {
                History = new ImportHistoryModel
                {
                    SubscriptionId = subscription.Id,
                    Completed = Timestamp.FromDateTime(DateTime.UtcNow),
                    Success = response.Success
                }
            };

            subscription.Imports.ForEach(i => historyRequest.History.Imports.Add(i));

            return historyRequest;
        }
    }
}
