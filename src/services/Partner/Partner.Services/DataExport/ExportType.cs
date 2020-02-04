using System;

namespace Partner.Services.DataExport
{
    [Flags]
    public enum ExportType
    {        
        Demographics = 0x01,
        Firmographics = 0x02,
        Accounts = 0x04,
        AccountTransactions = 0x08,
        LoanAccounts = 0x10,
        LoanTransactions = 0x20,
        LoanCollateral = 0x40,
        LoanApplications = 0x80,
        LoanAttributes = 0x100
    }
}
