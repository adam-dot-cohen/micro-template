using System;

namespace Laso.Domain.Models
{
    public class LoanTransaction : Entity<string>
    {
        public string Id { get; set; }
        public string LoanAccountId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Amount { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
    }
}
