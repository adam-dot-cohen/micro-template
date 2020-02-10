using System.ComponentModel.DataAnnotations;

namespace DataImport.Domain.Api
{
    public class Partner
    {        
        public string Id { get; set; }
        public string Name { get; set; }
        public PartnerIdentifier InternalIdentifier { get; set; }
    }
}
