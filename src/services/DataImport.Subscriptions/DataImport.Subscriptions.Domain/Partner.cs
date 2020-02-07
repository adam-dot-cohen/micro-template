using System.ComponentModel.DataAnnotations;

namespace DataImport.Subscriptions.Domain
{
    public class Partner
    {
        [Required]
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
