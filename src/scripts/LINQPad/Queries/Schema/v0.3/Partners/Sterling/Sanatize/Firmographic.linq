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
	var inputFileName = $@"{filePath}\SterlingNational_Laso_R_Firmographic_11107019_11107019.csv";
	var outputFileName = Path.ChangeExtension(inputFileName, ".sanitized.csv");
	
	var container = IoC.Initialize();
	var reader = container.GetInstance<IDelimitedFileReader>();
	
	List<Firmographic> records;
	using (var inputStream = File.OpenRead(inputFileName))
	{
		reader.Open(inputStream);
		
		records = reader
			.ReadRecords(new
			{
				LASOCATEGORY = (string)null,
				BUSINESS_ID = (string)null,
				ClientKey_id = (string)null,
				EFFECTIVE_DATE = (string)null,
				DATE_STARTED = (string)null,
				INDUSTRY_NAICS = (string)null,
				INDUSTRY_SIC = (string)null,
				BUSINESS_TYPE = (string)null,
				LEGAL_BUSINESS_NAME = (string)null,
				BUSINESS_PHONE = (string)null,
				EIN = (string)null,
				ADDRESS_ZIP_CODE = (string)null
			})
//			.Take(5)
//			.ToList()
//			.Dump("Raw", toDataGrid: true)
			.Select(r => new Firmographic
			{
				Business_Id = r.BUSINESS_ID.TrimmedOrNullIfWhiteSpace(),
				Customer_Id = r.ClientKey_id.TrimmedOrNullIfWhiteSpace(),
				Effective_Date = r.EFFECTIVE_DATE.ToTranslatedDateTime(),
				Date_Started = r.DATE_STARTED.ToTranslatedDateTime(false),
				Industry_NAICS = r.INDUSTRY_NAICS.TrimmedOrNullIfWhiteSpace(false),
				Industry_SIC = r.INDUSTRY_SIC.TrimmedOrNullIfWhiteSpace(false),
				Business_Type = r.BUSINESS_TYPE.TrimmedOrNullIfWhiteSpace(false),
				Legal_Business_Name = r.LEGAL_BUSINESS_NAME.TrimmedOrNullIfWhiteSpace(false),
				//Business_Phone = r.BUSINESS_PHONE.TrimmedOrNullIfWhiteSpace(false),
				//EIN = r.EIN.TrimmedOrNullIfWhiteSpace(false),
				Postal_Code = r.ADDRESS_ZIP_CODE.TrimmedOrNullIfWhiteSpace(false)
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

public class Firmographic
{
	public string Business_Id { get; set; }
	public string Customer_Id { get; set; }
	public DateTime? Effective_Date { get; set; }
	public DateTime? Date_Started { get; set; }
	public string Industry_NAICS { get; set; }
	public string Industry_SIC { get; set; }
	public string Business_Type { get; set; }
	public string Legal_Business_Name { get; set; }
	public string Business_Phone { get; set; }
	public string EIN { get; set; }
	public string Postal_Code { get; set; }
}