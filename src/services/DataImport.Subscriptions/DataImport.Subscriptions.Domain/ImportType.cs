using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DataImport.Subscriptions.Domain
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