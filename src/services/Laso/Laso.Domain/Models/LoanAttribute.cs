namespace Laso.Domain.Models
{
    public class LoanAttribute : Entity<string>
    {
        public string Id { get; set; }
        public string ApplicationId { get; set; }
        public string LoanAccountId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }
}
