using System;

namespace Laso.Domain.Models
{
    public class AccountTransaction : Entity<string>
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PostedDate { get; set; }
    }
}
