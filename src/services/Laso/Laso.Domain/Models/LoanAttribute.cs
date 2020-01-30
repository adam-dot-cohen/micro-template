using System;
using System.Collections.Generic;
using System.Text;

namespace Laso.Domain.Models
{
    public class LoanAttribute : Entity<string>
    {
        public string Id { get; set; }
        public LoanApplication Application { get; set; }
        public LoanAccount Account { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }
}
