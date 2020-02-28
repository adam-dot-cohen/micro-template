using System;
using System.Collections.Generic;

namespace Laso.DataImport.Services.DTOs
{
    public class ImportSubscription : Dto<string>
    {
        public string Id { get; set; }
        public string PartnerId { get; set; }
        public ImportFrequency Frequency { get; set; }
        public IEnumerable<ImportType> Imports { get; set; }
        public DateTime? LastSuccessfulImport { get; set; }
        public DateTime? NextScheduledImport { get; set; }
        public FileType OutputFileType { get; set; }
        public EncryptionType EncryptionType { get; set; }
        public string IncomingStorageLocation { get; set; }
        public string IncomingFilePath { get; set; }
    }

    public enum FileType
    {
        CSV,
    }

    public enum EncryptionType
    {
        None,
        PGP
    }
}
