using System;

namespace Partner.Domain.Quarterspot.Enumerations
{
    [Flags]
    public enum BusinessType
    {
        None = 0,
        Borrower = 1,
        Investor = 2,
        Affiliate = 4,
        Lender = 8,
        Servicer = 16,
        [Obsolete] ScoringPortal = 32,
        Tenant = 64,
        Collection = 128
    }
}
