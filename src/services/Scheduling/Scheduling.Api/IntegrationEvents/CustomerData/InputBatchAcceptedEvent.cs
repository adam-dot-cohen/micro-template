namespace Laso.Scheduling.Api.IntegrationEvents.CustomerData
{
    public class InputBatchAcceptedEventV1
    {
        public InputBatchAcceptedEventV1(string partnerId)
        {
            PartnerId = partnerId;
        }

        public string PartnerId { get; }
    }
}
