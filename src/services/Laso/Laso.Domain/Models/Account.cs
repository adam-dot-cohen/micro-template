using System;
using System.Collections.Generic;
using System.Text;

namespace Laso.Domain.Models
{
    public class Account : Entity<string>
    {
        public string Id { get; set; }
        public Business Business { get; set; }
        public Customer Customer { get; set; }
        public string AccountType { get; set; }
        public decimal InterestRate { get; set; }
        public decimal CurrentBalance { get; set; }
        public IEnumerable<AccountTransaction> Transactions { get; set; }
    }
}
