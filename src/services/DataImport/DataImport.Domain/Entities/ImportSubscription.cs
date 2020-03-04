using System;
using System.Collections.Generic;
using System.Linq;

namespace Laso.DataImport.Domain.Entities
{
    public class ImportSubscription : TableStorageEntity
    {
        public string PartnerId { get; set; }
        public ImportFrequency Frequency { get; set; }
        [Delimited]
        public IEnumerable<string> Imports { get; set; }
        public DateTime? LastSuccessfulImport { get; set; }
        public DateTime? NextScheduledImport { get; set; }
        public FileType OutputFileType { get; set; }
        public EncryptionType EncryptionType { get; set; }
        public string IncomingStorageLocation { get; set; }
        public string IncomingFilePath { get; set; }

        public IEnumerable<ImportType> GetImports()
        {
            return Imports?.Select(Enum.Parse<ImportType>);
        }
    }

    public enum FileType
    {
        Csv,
    }

    public enum EncryptionType
    {
        None,
        Pgp
    }
}
