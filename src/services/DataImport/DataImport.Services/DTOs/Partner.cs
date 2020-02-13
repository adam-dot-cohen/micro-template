using System.ComponentModel.DataAnnotations;

namespace DataImport.Services.DTOs
{
    public class Partner : Dto<string>
    {        
        public string Id { get; set; }
        public string Name { get; set; }
        public PartnerIdentifier InternalIdentifier { get; set; }
    }
}
