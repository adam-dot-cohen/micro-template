using System.ComponentModel.DataAnnotations;

namespace DataImport.Domain
{
    public class Partner
    {
        [Required]
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
