using System;

namespace DataImport.Domain.Api
{
    public enum FileType
    {
        CSV,
    }

    public enum EncryptionType
    {
        None,
        PGP
    }

    public class ImportStorageConfiguration
    {
        public string Id { get; set; }
        public string PartnerId { get; set; }
        public FileType OutputFileType { get; set; }
        public EncryptionType EncryptionType { get; set; }
        public string IncomingStorageLocation { get; set; }
        public string OutgoingStorageLocation { get; set; }
    }
}
