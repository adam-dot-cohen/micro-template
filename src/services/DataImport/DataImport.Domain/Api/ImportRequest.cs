namespace DataImport.Domain.Api
{
    public class ImportRequest
    {
        public PartnerIdentifier Partner { get; set; }
        public ImportType[] Imports { get; set; }
        public ImportFrequency Frequency { get; set; }
    }
}
