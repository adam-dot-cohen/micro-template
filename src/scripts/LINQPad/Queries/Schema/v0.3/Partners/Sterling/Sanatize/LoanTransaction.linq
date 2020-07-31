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
	var inputFileName = $@"{filePath}\SterlingNational_Laso_R_LoanTransaction_11107019_11107019.csv";
	var outputFileName = Path.ChangeExtension(inputFileName, ".sanitized.csv");
	
	var container = IoC.Initialize();
	var reader = container.GetInstance<IDelimitedFileReader>();
	
	List<LoanTransaction> records;

	using (var stream = File.OpenRead(inputFileName))
	{
		reader.Open(stream);

		records = reader
			.ReadRecords(new
			{
				LASO_CATEGORY = "",
				TranKey_id = "",
				ACCTKey_id = "",
				TRANSACTION_DATE = "",
				AMOUNT = "",
				CATEGORY = "",
				DESCRIPTION = ""
			})
//			.Take(10)
//			.ToList()
//			.Dump("Raw", toDataGrid: true)
			.Select(r => new LoanTransaction
			{
				Loan_Transaction_Id = r.TranKey_id,
				Loan_Account_Id = r.ACCTKey_id,
				Transaction_Date = r.TRANSACTION_DATE.ToTranslatedDateTime(),
				Amount = r.AMOUNT.ToTranslatedCurrency(),
				Category = r.CATEGORY.TrimmedOrNullIfWhiteSpace(r.DESCRIPTION.IsNullOrEmpty()),         // Required if no description
			Description = r.DESCRIPTION.TrimmedOrNullIfWhiteSpace(r.CATEGORY.IsNullOrWhiteSpace())  // Required if no category
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

public class LoanTransaction
{
	public string Loan_Transaction_Id { get; set; }
	public string Loan_Account_Id { get; set; }
	public DateTime? Transaction_Date { get; set; }
	public decimal? Amount { get; set; }
	public string Category { get; set; }
	public string Description { get; set; }
}