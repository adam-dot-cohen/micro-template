using System;

namespace Partner.Services.DataExport
{
    [Flags]
    public enum ExportType
    {        
        Demographic = 0x01,
        Firmographic = 0x02,
        Account = 0x04,
        AccountTransaction = 0x08,
        LoanAccount = 0x10,
        LoanTransaction = 0x20,
        LoanCollateral = 0x40,
        LoanApplication = 0x80,
        LoanAttribute = 0x100
    }
}
