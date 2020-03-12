namespace Laso.Identity.Core.IntegrationEvents
{
    public class PartnerCreatedEventV1 : IIntegrationEvent
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
    }
}
