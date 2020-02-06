namespace DataImport.Domain.Laso.Models
{
    public class LoanAttribute : ILasoEntity
    {
        public string LoanAttributeId { get; set; }
        public string ApplicationId { get; set; }
        public string LoanAccountId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }
}
