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
	var inputFileName = $@"{filePath}\SterlingNational_Laso_R_LoanCollateral_11107019_11107019.csv";
	var outputFileName = Path.ChangeExtension(inputFileName, ".sanitized.csv");
	
	var container = IoC.Initialize();
	var reader = container.GetInstance<IDelimitedFileReader>();
	
	List<LoanCollateral> records;

	using (var stream = File.OpenRead(inputFileName))
	{
		reader.Open(stream);

		records = reader
			.ReadRecords(new
			{
				LASOCATEGORY = "",
				COLLKey_id = "",
				ACCTKey_id = "",
				TYPE = "",
				DESCRIPTION = "",
				INITIAL_ASSESSED_VALUE = "",
				INITIAL_ASSESSED_DATE = "",
				ASSESSED_VALUE = "",
				ASSESSED_DATE = "", 
				APPL_ID = "",
				COLLATERAL_CODE = "",
				COLLATERAL_CATEGORY = "",
				POST_DATE = ""
			})
//			.Take(10)
//			.ToList()
//			.Dump("Raw", toDataGrid: true)
			.Select(r => new LoanCollateral
			{
				Loan_Collateral_Id = r.COLLKey_id.TrimmedOrNullIfWhiteSpace(),
				Loan_Account_Id = r.ACCTKey_id.TrimmedOrNullIfWhiteSpace(),
				Type = r.TYPE.TrimmedOrNullIfWhiteSpace(false),
				Description = r.DESCRIPTION.TrimmedOrNullIfWhiteSpace(false),
				Initial_Assessed_Value = r.INITIAL_ASSESSED_VALUE.ToTranslatedCurrency(),
				Initial_Assessed_Date = r.INITIAL_ASSESSED_DATE.ToTranslatedDateTime(false),
				Assessed_Value = r.ASSESSED_VALUE.ToTranslatedCurrency(),
				Assessed_Date = r.ASSESSED_DATE.ToTranslatedDateTime(false)
			})
			.ToList();
	}
	
//	records.Dump("Translated", toDataGrid: true);

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
}

public class LoanCollateral
{
	public string Loan_Collateral_Id { get; set; }
	public string Loan_Account_Id { get; set; }
	public string Type { get; set; }
	public string Description { get; set; }
	public decimal? Initial_Assessed_Value { get; set; }
	public DateTime? Initial_Assessed_Date { get; set; }
	public decimal? Assessed_Value { get; set; }
	public DateTime? Assessed_Date { get; set; }
}