namespace Laso.IntegrationEvents.AzureServiceBus
{
    public class AzureServiceBusConfiguration
    {
        public string TopicNameFormat { get; set; } = "{EventName}";
    }
}