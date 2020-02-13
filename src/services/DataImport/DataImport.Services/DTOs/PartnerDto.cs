using System.ComponentModel.DataAnnotations;

namespace DataImport.Services.DTOs
{
    public class PartnerDto : Dto<string>
    {        
        public string Id { get; set; }
        public string Name { get; set; }
        public PartnerIdentifierDto InternalIdentifier { get; set; }
    }
}
