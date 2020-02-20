using System;

namespace Laso.DataImport.Domain.Models
{
    public class LoanCollateral : ILasoEntity
    {
        public string LoanCollateralId { get; set; }
        public string LoanAccountId { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string InitialAssessedValue { get; set; }
        public DateTime InitialAssessedDate { get; set; }
        public string AssessedValue { get; set; }
        public DateTime AssessedDate { get; set; }
    }
}
