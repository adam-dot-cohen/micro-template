using Laso.IntegrationEvents;

namespace Laso.Scheduling.Core.IntegrationEvents
{
    public class ExperimentRunScheduledEventV1 : IIntegrationEvent
    {
        public ExperimentRunScheduledEventV1(string partnerId)
        {
            PartnerId = partnerId;
        }

        public string PartnerId { get; }
    }
}
