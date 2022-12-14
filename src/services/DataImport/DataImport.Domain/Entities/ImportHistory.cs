using System;
using System.Collections.Generic;
using System.Linq;

namespace Laso.DataImport.Domain.Entities
{
    public class ImportHistory : TableStorageEntity
    {
        public string SubscriptionId { get; set; }
        public DateTime Completed { get; set; }
        public bool Success { get; set; }
        [Delimited]
        public IEnumerable<string> FailReasons { get; set; }
        [Delimited]
        public IEnumerable<ImportType> Imports { get; set; }
    }
}
