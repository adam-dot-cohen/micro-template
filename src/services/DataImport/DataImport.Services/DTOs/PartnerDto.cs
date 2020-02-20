using System.ComponentModel.DataAnnotations;

namespace Laso.DataImport.Services.DTOs
{
    public class PartnerDto : Dto<string>
    {        
        public string Id { get; set; }
        public string Name { get; set; }
        public PartnerIdentifier InternalIdentifier { get; set; }
    }
}
