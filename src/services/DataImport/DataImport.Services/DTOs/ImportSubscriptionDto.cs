using System;
using System.Collections.Generic;

namespace Laso.DataImport.Services.DTOs
{
    public class ImportSubscriptionDto : Dto<string>
    {
        public string Id { get; set; }
        public string PartnerId { get; set; }
        public ImportFrequencyDto Frequency { get; set; }
        public IEnumerable<ImportTypeDto> Imports { get; set; }
        public DateTime? LastSuccessfulImport { get; set; }
        public DateTime? NextScheduledImport { get; set; }
        public FileTypeDto OutputFileType { get; set; }
        public EncryptionTypeDto EncryptionType { get; set; }
        public string IncomingStorageLocation { get; set; }
        public string IncomingFilePath { get; set; }
    }

    public enum FileTypeDto
    {
        CSV,
    }

    public enum EncryptionTypeDto
    {
        None,
        PGP
    }
}
