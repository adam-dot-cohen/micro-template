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
	var inputFileName = $@"{filePath}\SterlingNational_Laso_R_AccountTransaction_11072019_01012016.escaped.csv";
	var outputFileName = Path.ChangeExtension(inputFileName, ".sanitized.csv");
	
	var container = IoC.Initialize();
	var reader = container.GetInstance<IDelimitedFileReader>();
	
	List<AccountTransaction> records;
	using (var inputStream = File.OpenRead(inputFileName))
	{
		reader.Open(inputStream);

		records = reader
			.ReadRecords(new
			{
				AcctTranKey_id = "",
				ACCTKey_id = "",
				TRANSACTION_DATE = "",
				POST_DATE = "",
				TRANSACTION_CATEGORY = "",
				AMOUNT = "",
				MEMO_FIELD = "",
				MCC_CODE = ""
			})
			//.Take(100)
			//.ToList()
			//.Dump("Raw", toDataGrid: true)
			.Select(r => new AccountTransaction
			{
				Transaction_Id = r.AcctTranKey_id.TrimmedOrNullIfWhiteSpace(),
				Account_Id = r.ACCTKey_id.TrimmedOrNullIfWhiteSpace(),
				Transaction_Date = r.TRANSACTION_DATE.TransformDateTime(),
				Post_Date = r.POST_DATE.TransformDateTime(),
				Transaction_Category = r.TRANSACTION_CATEGORY.TrimmedOrNullIfWhiteSpace(false),
				Amount = r.AMOUNT.TransformCurrency(false),
				Memo_Field = r.MEMO_FIELD.TrimmedOrNullIfWhiteSpace(false), // Should be required!
				MCC_Code = r.MCC_CODE.TrimmedOrNullIfWhiteSpace(false),
				//Balance_After_Transaction = 
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

	public static DateTime? TransformDateTime(this string input, bool required = true)
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

	public static decimal? TransformCurrency(this string input, bool required = true)
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

	public static int? TransformCount(this string input, bool required = true)
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

public class AccountTransaction
{
	public string Transaction_Id { get; set; }
	public string Account_Id { get; set; }
	public DateTime? Transaction_Date { get; set; }
	public DateTime? Post_Date { get; set; }
	public string Transaction_Category { get; set; }
	public decimal? Amount { get; set; }
	public string Memo_Field { get; set; }
	public string MCC_Code { get; set; }
	public decimal? Balance_After_Transaction { get; set; }
}