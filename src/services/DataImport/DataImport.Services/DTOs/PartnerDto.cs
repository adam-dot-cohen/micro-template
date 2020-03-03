using System.ComponentModel.DataAnnotations;

namespace Laso.DataImport.Services.DTOs
{
    public class PartnerDto : IDto<string>
    {        
        public string Id { get; set; }
        public string Name { get; set; }
        public PartnerIdentifier InternalIdentifier { get; set; }
    }
}
