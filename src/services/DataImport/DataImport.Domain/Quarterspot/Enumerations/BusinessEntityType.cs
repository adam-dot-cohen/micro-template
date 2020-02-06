

namespace DataImport.Domain.Laso.Quarterspot.Enumerations
{
    public class BusinessEntityType : Enumeration<BusinessEntityType>
    {
        public static readonly BusinessEntityType SoleProprietorship =
            new BusinessEntityType(0, "Sole Proprietorship", keywords: new[] { "PROPRIETORSHIP", "SOLE PROP", "Individual Sole Prop" });

        public static readonly BusinessEntityType ProfessionalCorporation =
            new BusinessEntityType(1, "Professional Corporation, PC, or PLLC", keywords: new[] { "PROFESSIONAL", "PROFESSIONAL CORPORATION", "CORPORATION PROFESSIONAL", "PROFESSIONAL LIMITED LIABILITY", "PC", "PLLC", "DOMESTIC PROFESSIONAL", "Medical Legal Corp" });

        public static readonly BusinessEntityType PartnershipOrLlc =
            new BusinessEntityType(2, "Partnership or LLC", keywords: new[] { "LIMITED LIABILITY CORPORATION", "LIMITED LIABILITY", "LIMITED LIABILITY COMPANY", "LLC", "PARTNERSHIP", "LIMITED PARTNERSHIP" });

        public static readonly BusinessEntityType Corporation =
            new BusinessEntityType(3, "Corporation (INC Only)", keywords: new[] { "CORP", "CORPORATION-BUSINESS", "INC", "CORPORATION", "CORPORATION PROFIT", "BUSINESS CORPORATION", "REGULAR CORPORATION", "FOR PROFIT", "FOR PROFIT CORPORATION", "PROFIT", "PROFIT CORPORATION", "BCA", "CODE 490", "CODE 490 PROFIT" });

        public static readonly BusinessEntityType GovernmentAgency =
            new BusinessEntityType(4, "Government Agency", true, new[] { "Government", "Government Agency", "Gov Agency" });

        public static readonly BusinessEntityType Exempt =
            new BusinessEntityType(5, "Non-Profit", true, new[] { "EXEMPT", "EXEMPT CORPORATION", "Tax Exempt", "Tax Exempt 501C", "501C", "NON PROFIT", "NONPROFIT", "NONPROFIT CORPORATION" });

        public BusinessEntityType(int value, string displayName, bool isProhibited = false, string[] keywords = null) : base(value, displayName)
        {
            IsProhibited = isProhibited;
            Keywords = keywords;
        }

        public bool IsProhibited { get; set; }
        public string[] Keywords { get; set; }

        public static implicit operator BusinessEntityType(int value)
        {
            return FromValue(value);
        }     
    }
}
