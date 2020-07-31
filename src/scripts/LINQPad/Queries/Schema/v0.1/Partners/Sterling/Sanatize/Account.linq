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
	var inputFileName = $@"{filePath}\SterlingNational_Laso_R_Account_11072019_01012017.csv";
	var outputFileName = Path.ChangeExtension(inputFileName, ".sanitized.csv");
	
	var container = IoC.Initialize();
	var reader = container.GetInstance<IDelimitedFileReader>();
	
	List<Account> records;
	using (var inputStream = File.OpenRead(inputFileName))
	{
		reader.Open(inputStream);
		
		records = reader
			.ReadRecords(new
			{
				LASO_CATEGORY = "",
				ACCTKey_id = "",
				BUSINESS_ID = "",
				ClientKey_id = "",
				ACCOUNT_TYPE = "",
				INTEREST_RATE_METHOD = "",
				INTEREST_RATE = "",
				ACCOUNT_OPEN_DATE = "",
				CURRENT_BALANCE = "",
				AVERAGE_DAILY_BALANCE = "",
				ACCOUNT_CLOSED_DATE = "",
				ACCOUNT_CLOSED_REASON = "",
				PRODUCT_CODE = "",
				PRODUCT_DESCRIPTION = "",
				PRODUCT_KEY = "",
				ACCOUNT_STATUS = "",
				EFFECTIVE_DATE = ""
			})
			//.Take(5)
			//.ToList()
			//.Dump("Raw", toDataGrid: true)
			.Select(r => new Account
			{
				Account_Id = r.ACCTKey_id.TrimmedOrNullIfWhiteSpace(),
				Business_Id = r.BUSINESS_ID.TrimmedOrNullIfWhiteSpace(false /*r.ClientKey_id.IsNotNullOrWhiteSpace()*/),
				Customer_Id = r.ClientKey_id.TrimmedOrNullIfWhiteSpace(false /*r.BUSINESS_ID.IsNullOrWhiteSpace()*/),
				Effective_Date = r.EFFECTIVE_DATE.ToTranslatedDateTime(false),
				Account_Type = r.ACCOUNT_TYPE.TrimmedOrNullIfWhiteSpace(false),
				Interest_Rate_Method = r.INTEREST_RATE_METHOD.TrimmedOrNullIfWhiteSpace(false),
				Interest_Rate = r.INTEREST_RATE.ToTranslatedCurrency(false),
				Account_Open_Date = r.ACCOUNT_OPEN_DATE.ToTranslatedDateTime(false),
				Current_Balance = r.CURRENT_BALANCE.ToTranslatedCurrency(false),
				Current_Balance_Date = null, // TODO: Missing
				Average_Daily_Balance = r.AVERAGE_DAILY_BALANCE.ToTranslatedCurrency(false),
				Account_Closed_Date = r.ACCOUNT_CLOSED_DATE.ToTranslatedDateTime(false),
				Account_Closed_Reason = r.ACCOUNT_CLOSED_REASON.TrimmedOrNullIfWhiteSpace(false /*r.ACCOUNT_CLOSED_DATE.IsNotNullOrWhiteSpace()*/)
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
	public static string TrimmedOrNullIfWhiteSpace(this string input, bool required = true)
	{
		var trimmedInput = input.Trim();

		if (trimmedInput.IsNullOrWhiteSpace() || (trimmedInput == "NULL"))
		{
			if (required)
				throw new InvalidOperationException($"Invalid input: '{input}'");
			else
				return null;
		}

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
	
	public static int? ToTranslatedCreditScore(this string input, bool required = true)
	{
		var checkedInput = input.TrimmedOrNullIfWhiteSpace(required);
		
		if ((checkedInput == null) && !required)
			return null;

		if (!checkedInput.All(char.IsDigit))
			return null;

		// Enusure FICO Score Range (300-850)
		if (!int.TryParse(checkedInput, out var score))
			return null;

		if ((score < 300) || (score > 850))
		{
			if (required)
				throw new InvalidOperationException($"Invalid credit score: '{input}'");
			else
				return null;
		}

		return score;
	}
}

public class Account
{
	public string Account_Id { get; set; }
	public string Business_Id { get; set; }
	public string Customer_Id { get; set; }
	public DateTime? Effective_Date { get; set; }
	public string Account_Type { get; set; }
	public string Interest_Rate_Method { get; set; }
	public decimal? Interest_Rate { get; set; }
	public DateTime? Account_Open_Date { get; set; }
	public decimal? Current_Balance { get; set; }
	public DateTime? Current_Balance_Date { get; set; }
	public decimal? Average_Daily_Balance { get; set; }
	public DateTime? Account_Closed_Date { get; set; }
	public string Account_Closed_Reason { get; set; }
}