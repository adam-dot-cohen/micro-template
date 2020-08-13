namespace IntegrationMessages.AzureServiceBus
{
    public class AzureServiceBusMessageConfiguration
    {
        public string QueueNameFormat { get; set; } = "{CommandName}";
        public string ConnectionString { get; set; }
    }
}