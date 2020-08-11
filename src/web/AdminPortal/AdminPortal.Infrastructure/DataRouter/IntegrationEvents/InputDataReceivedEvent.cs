using Laso.IntegrationEvents;

namespace Laso.AdminPortal.Infrastructure.DataRouter.IntegrationEvents
{
    public class InputDataReceivedEventV1 : IIntegrationEvent
    {
        public string Uri { get; set; }
        public string ETag { get; set; }
        public string ContentType { get; set; }
        public long ContentLength { get; set; }
    }
}