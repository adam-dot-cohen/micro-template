<Query Kind="Program">
  <Reference>D:\Repos\Platform\src\QS.Admin\bin\EntityFramework.dll</Reference>
  <Reference>D:\Repos\Platform\src\QS.Admin\bin\EntityFramework.SqlServer.dll</Reference>
  <Reference>D:\Repos\Platform\src\QS.Admin\bin\Infrastructure.dll</Reference>
  <Reference>D:\Repos\Platform\src\QS.Admin\bin\QS.Core.dll</Reference>
  <Reference>D:\Repos\Platform\src\QS.Admin\bin\StructureMap.dll</Reference>
  <Namespace>Infrastructure.IoC</Namespace>
  <Namespace>QS.Core.Infrastructure.IO</Namespace>
  <Namespace>QS.Core.Extensions</Namespace>
  <CopyLocal>true</CopyLocal>
</Query>

void Main()
{
	var filePath = @"E:\Insights\Partners\SNB\Data\Decrypted";
	var inputFileName = $@"{filePath}\SterlingNational_Laso_R_LoanAccount_11107019_11107019.csv";
	var outputFileName = Path.ChangeExtension(inputFileName, ".sanitized.csv");

	var container = IoC.Initialize();
	var reader = container.GetInstance<IDelimitedFileReader>();

	List<LoanAccount> records;

	using (var stream = File.OpenRead(inputFileName))
	{
		reader.Open(stream);

		records = reader
			.ReadRecords(new
			{
				LASOCATEGORY = "",
				ACCTKey_id = "",
				BUSINESS_ID = "",
				ClientKey_id = "",
				PRODUCT_TYPE = "",
				EFFECTIVE_DATE = "",
				ISSUE_DATE = "",
				MATURITY_DATE = "",
				INTEREST_RATE_METHOD = "",
				INTEREST_RATE = "",
				AMORTIZATION_METHOD = "",
				TERM = "",
				INSTALLMENT = "",
				INSTALLMENT_FREQUENCY = "",
				REFINANCE_LOAN_ID = "",
				ORIGINAL_PRINCIPAL_BALANCE = "",
				CURRENT_PRINCIPAL_BALANCE = "",
				CURRENT_INTEREST_BALANCE_XYTD = "",
				ORIGINAL_CREDIT_LIMIT = "",
				CURRENT_CREDIT_LIMIT_XUNUSED = "",
				PAYMENT_AMOUNT = "",
				PAYMENT_INTEREST = "",
				PAYMENT_PRINCIPAL = "",
				WRITE_OFF_DATE = "",
				WRITE_OFF_PRIN_BAL_XALL = "",
				WRITE_OFF_INTEREST_BALANCE = "",
				RECOVERY_PRINCIPAL_BALANCE = "",
				RECOVERY_PRINCIPAL_DATE = "",
				CLOSE_DATE = "",
				CLOSE_REASON = "",
				PAST_DUE_01_TO_29_DAYS_COUNT = "",
				PAST_DUE_30_TO_59_DAYS_COUNT = "",
				PAST_DUE_60_TO_89_DAYS_COUNT = "",
				PAST_DUE_OVER_90_DAYS_COUNT = "",
				TOTAL_DAYS_PAST_DUE = ""
			})
			//		.Take(100)
			//		.ToList()
			//		.Dump("Raw", toDataGrid: true)
			.Select(r => new LoanAccount
			{
				Loan_Account_Id = r.ACCTKey_id,
				Business_Id = r.BUSINESS_ID,
				Customer_Id = r.ClientKey_id,
				Product_Type = r.PRODUCT_TYPE.TrimmedOrNullIfWhiteSpace(),
				Effective_Date = r.EFFECTIVE_DATE.ToTranslatedDateTime(),
				Issue_Date = r.ISSUE_DATE.ToTranslatedDateTime(false),
				Maturity_Date = r.MATURITY_DATE.ToTranslatedDateTime(false),
				Interest_Rate_Method = r.INTEREST_RATE_METHOD.TrimmedOrNullIfWhiteSpace(),
				Interest_Rate = r.INTEREST_RATE,
				Amortization_Method = r.AMORTIZATION_METHOD.TrimmedOrNullIfWhiteSpace(),
				Term = r.TERM,
				Installment = r.INSTALLMENT.TrimmedOrNullIfWhiteSpace(),
				Installment_Frequency = r.INSTALLMENT_FREQUENCY.TrimmedOrNullIfWhiteSpace(),
				Refinanced_Loan_Id = r.REFINANCE_LOAN_ID.TrimmedOrNullIfWhiteSpace(),
				Original_Principal_Balance = r.ORIGINAL_PRINCIPAL_BALANCE.ToTranslatedCurrency(),
				Current_Principal_Balance = r.CURRENT_PRINCIPAL_BALANCE.ToTranslatedCurrency(),
				Current_Interest_Balance = r.CURRENT_INTEREST_BALANCE_XYTD.ToTranslatedCurrency(),
				Original_Credit_Limit = r.ORIGINAL_CREDIT_LIMIT.ToTranslatedCurrency(),
				Current_Credit_Limit = r.CURRENT_CREDIT_LIMIT_XUNUSED.ToTranslatedCurrency(),
				Payment_Amount = r.PAYMENT_AMOUNT.ToTranslatedCurrency(),
				Payment_Interest = r.PAYMENT_INTEREST.ToTranslatedCurrency(),
				Payment_Principal = r.PAYMENT_PRINCIPAL.ToTranslatedCurrency(),
				Write_Off_Date = r.WRITE_OFF_DATE.ToTranslatedDateTime(false),
				Write_Off_Principal_Balance = r.WRITE_OFF_PRIN_BAL_XALL.ToTranslatedCurrency(r.WRITE_OFF_DATE.IsNotNullOrWhiteSpace()),
				Write_Off_Interest_Balance = r.WRITE_OFF_INTEREST_BALANCE.ToTranslatedCurrency(r.WRITE_OFF_DATE.IsNotNullOrWhiteSpace()),
				Recovery_Principal_Balance = r.RECOVERY_PRINCIPAL_BALANCE.ToTranslatedCurrency(false),
				Recovery_Principal_Date = r.RECOVERY_PRINCIPAL_DATE.ToTranslatedDateTime(false),
				Close_Date = r.CLOSE_DATE.ToTranslatedDateTime(false),
				Close_Reason = r.CLOSE_REASON.TrimmedOrNullIfWhiteSpace(),
				Past_Due_01_to_29_Days_Count = r.PAST_DUE_01_TO_29_DAYS_COUNT.ToTranslatedCount(false),
				Past_Due_30_to_59_Days_Count = r.PAST_DUE_30_TO_59_DAYS_COUNT.ToTranslatedCount(false),
				Past_Due_60_to_89_Days_Count = r.PAST_DUE_60_TO_89_DAYS_COUNT.ToTranslatedCount(false),
				Past_Due_Over_90_Days_Count = r.PAST_DUE_OVER_90_DAYS_COUNT.ToTranslatedCount(false),
				Total_Days_Past_Due = r.TOTAL_DAYS_PAST_DUE.ToTranslatedCount(false)
			})
			.ToList();
	}
	
	//records.Dump("Translated", toDataGrid: true);

	var delimitedFileWriter = container.GetInstance<IDelimitedFileWriter>();
	using (var outputStream = File.OpenWrite(outputFileName))
	{
		delimitedFileWriter.Open(outputStream);
		delimitedFileWriter.WriteRecords(records);
	}
}

public static class StringNormalizationExtensions
{
	public static string TrimmedOrNullIfWhiteSpace(this string input)
	{
		var trimmedInput = input.Trim();
		
		if (trimmedInput.IsNullOrWhiteSpace() || (trimmedInput == "NULL"))
			return null;
			
		return trimmedInput;
	}

	public static DateTime? ToTranslatedDateTime(this string input, bool required = true)
	{
		var parsedDateTime = input.TryParseDateTime();
		if (parsedDateTime == default)
		{
			if (required)
				throw new InvalidOperationException($"Invalid Date/Time: '{input}'");
			else
				return null;
		}

		return parsedDateTime;
	}
	
	public static decimal? ToTranslatedCurrency(this string input, bool required = true)
	{
		var parsedCurrency = input.TryParseCurrency();
		if (parsedCurrency == default)
		{
			if (required)
				throw new InvalidOperationException($"Invalid currency: '{input}'");
			else
				return null;
		}
		
		return parsedCurrency;
	}
	
	public static int? ToTranslatedCount(this string input, bool required = true)
	{
		var parsedCount = input.TryParseInt32();
		if (parsedCount == default)
		{
			if (required)
				throw new InvalidOperationException($"Invalid count: '{input}'");
			else
				return null;
		}
		
		return parsedCount;
	}
}

public class LoanAccount
{
	public string Loan_Account_Id { get; set; }
	public string Business_Id { get; set; }
	public string Customer_Id { get; set; }
	public string Product_Type { get; set; }
	public DateTime? Effective_Date { get; set; }
	public DateTime? Issue_Date { get; set; }
	public DateTime? Maturity_Date { get; set; }
	public string Interest_Rate_Method { get; set; }
	public string Interest_Rate { get; set; }
	public string Amortization_Method { get; set; }
	public string Term { get; set; }
	public string Installment { get; set; }
	public string Installment_Frequency { get; set; }
	public string Refinanced_Loan_Id { get; set; }
	public decimal? Original_Principal_Balance { get; set; }
	public decimal? Current_Principal_Balance { get; set; }
	public decimal? Current_Interest_Balance { get; set; }
	public decimal? Original_Credit_Limit { get; set; }
	public decimal? Current_Credit_Limit { get; set; }
	public decimal? Payment_Amount { get; set; }
	public decimal? Payment_Interest { get; set; }
	public decimal? Payment_Principal { get; set; }
	public DateTime? Write_Off_Date { get; set; }
	public decimal? Write_Off_Principal_Balance { get; set; }
	public decimal? Write_Off_Interest_Balance { get; set; }
	public decimal? Recovery_Principal_Balance { get; set; }
	public DateTime? Recovery_Principal_Date { get; set; }
	public DateTime? Close_Date { get; set; }
	public string Close_Reason { get; set; }
	public int? Past_Due_01_to_29_Days_Count { get; set; }
	public int? Past_Due_30_to_59_Days_Count { get; set; }
	public int? Past_Due_60_to_89_Days_Count { get; set; }
	public int? Past_Due_Over_90_Days_Count { get; set; }
	public int? Total_Days_Past_Due { get; set; }
}