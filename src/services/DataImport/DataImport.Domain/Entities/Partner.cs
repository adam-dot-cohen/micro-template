namespace Laso.DataImport.Domain.Entities
{
    public class Partner : TableStorageEntity
    {
        public string Name { get; set; }
        public PartnerIdentifier InternalIdentifier { get; set; }
    }
}
