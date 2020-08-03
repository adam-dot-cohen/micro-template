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
	var inputFileName = $@"{filePath}\SterlingNational_Laso_R_Demographic_11107019_11107019.csv";
	var outputFileName = Path.ChangeExtension(inputFileName, ".sanitized.csv");
	
	var container = IoC.Initialize();
	var reader = container.GetInstance<IDelimitedFileReader>();
	
	List<Demographic> records;
	using (var inputStream = File.OpenRead(inputFileName))
	{
		reader.Open(inputStream);
		
		records = reader
			.ReadRecords(new
			{
				LASO_CATEGORY = (string)null,
				ClientKey_id = (string)null,
				BRANCH_ID = (string)null,
				CREDIT_SCORE = (string)null,
				CREDIT_SCORE_SOURCE = (string)null
			})
			//.Take(5)
			//.ToList()
			//.Dump("Raw", toDataGrid: true)
			.Select(r => new Demographic
			{
				Customer_Id = r.ClientKey_id.TrimmedOrNullIfWhiteSpace(),
				Branch_Id = r.BRANCH_ID.TrimmedOrNullIfWhiteSpace(false),
				Credit_Score = r.CREDIT_SCORE.TrimmedOrNullIfWhiteSpace(false),
				Credit_Score_Source = r.CREDIT_SCORE_SOURCE.TrimmedOrNullIfWhiteSpace(false)
			})
			.ToList();
	}
	
	//records.Dump("Translated", toDataGrid: true);
	//records.GroupBy(r => r.Customer_Id).Where(g => g.Count() > 2).ToList().Dump();

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

public string TranslateCreditScore(string creditScore)
{
	if (string.IsNullOrWhiteSpace(creditScore))
		return null;
	
	if (!creditScore.All(char.IsDigit))
		return null;
	
	// Enusure FICO Score Range (300-850)
	if (!int.TryParse(creditScore, out var score))
		return null;
	
	if ((score < 300) || (score > 850))
	{
		//$"Invalid Credit Score! {creditScore}".Dump();
		return null;
	}
	
	return creditScore;
}

public class Demographic
{
	public string Customer_Id { get; set; }
	public string Branch_Id { get; set; }
	public string Credit_Score { get; set; }
	public string Credit_Score_Source { get; set; }
}