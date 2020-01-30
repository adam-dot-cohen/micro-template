using System;
using System.Collections.Generic;
using System.Text;

namespace Laso.Domain.Models
{
    public class Customer : Entity<string>
    {
        public string Id { get; set; }
    }
}
