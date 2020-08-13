namespace Laso.IntegrationMessages.AzureStorageQueue
{
    public class AzureStorageQueueConfiguration
    {
        public string ServiceUrl { get; set; }
        public string QueueNameFormat { get; set; } = "{MessageName}";
    }
}