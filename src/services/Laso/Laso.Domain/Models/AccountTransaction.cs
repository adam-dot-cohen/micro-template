using System;
using System.Collections.Generic;
using System.Text;

namespace Laso.Domain.Models
{
    public class AccountTransaction : Entity<string>
    {
        public string Id { get; set; }
        public Account Account { get; set; }
        public decimal Amount { get; set; }
        public DateTime PostedDate { get; set; }
    }
}
