namespace Laso.Identity.Domain.Events
{
    public class PartnerCreatedEvent
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
    }
}
