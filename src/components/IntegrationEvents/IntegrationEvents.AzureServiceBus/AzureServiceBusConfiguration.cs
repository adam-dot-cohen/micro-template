namespace Laso.IntegrationEvents.AzureServiceBus
{
    //TODO: This should be moved into a shared AzureServiceBus library and/or we need to move Events (Messages) and (Commands) Messages into the same library . . . just a thought
    public class AzureServiceBusConfiguration
    {
        public string TopicNameFormat { get; set; } = "{EventName}";
        //Message conventions
        //public string QueueNameFormat { get; set; } = "{CommandName}";
        //public string ConnectionString { get; set; }
    }
}