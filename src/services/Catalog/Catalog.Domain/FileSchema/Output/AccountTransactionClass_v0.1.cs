using System;
using System.Collections.Generic;
using System.Text;

namespace Laso.Catalog.Domain.FileSchema.Output
{
    public class AccountTransactionClass_v0_1
    {
        public string Transaction_Id { get; set; } = null!;
        public long? Class { get; set; }
    }
}
