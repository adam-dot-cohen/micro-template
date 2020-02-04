namespace Laso.Domain.Models
{
    public class Account : Entity<string>
    {
        public string Id { get; set; }
        public string BusinessId { get; set; }
        public string CustomerId { get; set; }
        public string AccountType { get; set; }
        public decimal InterestRate { get; set; }
        public decimal CurrentBalance { get; set; }        
    }
}
