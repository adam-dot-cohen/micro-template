namespace Laso.IntegrationEvents.AzureServiceBus.Preview
{
    public class AzureServiceBusConfiguration
    {
        public string ServiceUrl { get; set; }
        public string TopicNameFormat { get; set; } = "{EventName}";
    }
}