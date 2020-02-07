using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace DataImport.Subscriptions.Domain
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ImportType
    {
        [EnumMember(Value = nameof(Demographic))]
        Demographic,
        [EnumMember(Value = nameof(Firmographic))]
        Firmographic,
        [EnumMember(Value = nameof(Account))] 
        Account,
        [EnumMember(Value = nameof(AccountTransaction))]
        AccountTransaction,
        [EnumMember(Value = nameof(LoanAccount))]
        LoanAccount,
        [EnumMember(Value = nameof(LoanTransaction))]
        LoanTransaction,
        [EnumMember(Value = nameof(LoanCollateral))]
        LoanCollateral,
        [EnumMember(Value = nameof(LoanApplication))]
        LoanApplication,
        [EnumMember(Value = nameof(LoanAttribute))]
        LoanAttribute
    }
}