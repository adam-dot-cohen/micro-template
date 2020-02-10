namespace Partner.Api.DTOs
{
    // [Ed S] need to avoid naming collisions between namespaces and entities (e.g. Partner.Domain.Partner is no good)
    // need a consistent guideline for naming entities, then change the below.
    public class PartnerDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string InternalIdentifier { get; set; }        
    }
}
