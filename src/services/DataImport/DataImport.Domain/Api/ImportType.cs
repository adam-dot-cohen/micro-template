using System.Text.Json.Serialization;

namespace DataImport.Domain.Api
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ImportType
    {
        Demographic,
        Firmographic,
        Account,
        AccountTransaction,
        LoanAccount,
        LoanTransaction,
        LoanCollateral,
        LoanApplication,
        LoanAttribute
    }
}
