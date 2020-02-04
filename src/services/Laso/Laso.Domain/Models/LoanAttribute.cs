namespace Laso.Domain.Models
{
    public class LoanAttribute : IEntity
    {
        public string LoanAttributeId { get; set; }
        public string ApplicationId { get; set; }
        public string LoanAccountId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }
}
