<Query Kind="Program">
  <Reference>D:\Repos\Platform\src\QS.Admin\bin\EntityFramework.dll</Reference>
  <Reference>D:\Repos\Platform\src\QS.Admin\bin\EntityFramework.SqlServer.dll</Reference>
  <Reference>D:\Repos\Platform\src\QS.Admin\bin\Infrastructure.dll</Reference>
  <Reference>D:\Repos\Platform\src\QS.Admin\bin\QS.Core.Base.dll</Reference>
  <Reference>D:\Repos\Platform\src\QS.Admin\bin\QS.Core.dll</Reference>
  <Reference>D:\Repos\Platform\src\QS.Admin\bin\StructureMap.dll</Reference>
  <Namespace>Infrastructure.IoC</Namespace>
  <Namespace>QS.Core.Extensions</Namespace>
  <Namespace>QS.Core.Infrastructure.IO</Namespace>
  <Namespace>QS.Core.Infrastructure.Matching</Namespace>
  <Namespace>QS.Core.Infrastructure.Mediator</Namespace>
  <Namespace>QS.Core.Base.String</Namespace>
  <CopyLocal>true</CopyLocal>
</Query>

void Main()
{
	var inputFileName = @"E:\Insights\Partners\SNB\Data\Decrypted\Demographic_LASO_M_Demographic_21-1-2020.csv";
	
	var container = IoC.Initialize();
	var reader = container.GetInstance<IDelimitedFileReader>();
	reader.Configuration.IgnoreMissingColumns = true;
	
	var validator = new DemographicValidator();
	
	using (var stream = File.OpenRead(inputFileName))
	{
		reader.Open(stream);
	
		var records = reader.ReadRecords<Demographic>()
			//.Take(100)
			.ToList();

		var results = records.Select(r => new
		{
			Record = r,
			Result = validator.Validate(r)
		})
		.ToList();
		
		var summary = new
		{
			Total_Record_Count = $"{results.Count():n0}",
			
			Valid_Record_Count = $"{results.Count(r => r.Result.IsValid):n0}",
			Invalid_Record_Count = $"{results.Count(r => !r.Result.IsValid):n0}",
			Invalid_Record_Errors = results.Where(r => !r.Result.IsValid)
				.SelectMany(r => r.Result.ValidationMessages.Select(m => $"{m.Key}: {m.Message}"))
				.GroupBy(m => m)
				.Select(g => new
				{
					Error = g.Key,
					Count = $"{g.Count():n0}"
				})
				.OrderBy(a => a.Error)
				.ToList(),

			Unique_Customer_Record_Count = $"{results.Select(r => r.Record.Customer_Id).Distinct().Count():n0}",
			Unique_Customer_Valid_Record_Count = $"{results.GroupBy(r => r.Record.Customer_Id).Where(g => g.Any(a => a.Result.IsValid)).Distinct().Count():n0}",
			Unique_Customer_Invalid_Record_Count = $"{results.GroupBy(r => r.Record.Customer_Id).Where(g => g.All(a => !a.Result.IsValid)).Distinct().Count():n0}",
		}
		.Dump("Demographic - Summary");
		
		results.Where(r => !r.Result.IsValid)
			.Select(r => new
			{
				r.Record.Customer_Id,
			
				r.Result.IsValid,
				ValidationMessages = r.Result.ValidationMessages.Select(m => $"{m.Key}: {m.Message}").Join(", ")
			})
			.ToList()
			.Dump("Demographic - Validation Errors", toDataGrid: true);
	}	
}

public abstract class Validator<T>
{
	protected ICollection<Func<T, ValidatorResult>> MemberValidations;
	
	public ValidatorResult Validate(T input)
	{
		var results = MemberValidations.Select(v => v(input));
		
		return new ValidatorResult
		{
			IsValid = results.All(r => r.IsValid),
			ValidationMessages = results.SelectMany(r => r.ValidationMessages).ToList()
		};
	}
}

public class ValidatorResult
{
	public bool IsValid { get; set; } = true;
	public ICollection<ValidatorMessage> ValidationMessages { get; set; } = new List<ValidatorMessage>();

	public void AddFailure(string key, string message)
	{
		IsValid = false;
		ValidationMessages.Add(new ValidatorMessage(key, message));
	}
}

public class ValidatorMessage
{
	public ValidatorMessage(string key, string message)
	{
		Key = key;
		Message = message;
	}

	public string Key { get; set; }
	public string Message { get; set; }
}

public class DemographicValidator : Validator<Demographic>
{
	public DemographicValidator()
	{
		MemberValidations = new List<Func<Demographic, ValidatorResult>>
		{
			d => UniqueIdentifier(d.Customer_Id, nameof(d.Customer_Id)),
			d => UniqueIdentifier(d.Branch_Id, nameof(d.Branch_Id)),
			d => Date(d.Effective_Date, nameof(d.Effective_Date)),
			d => CreditScore(d.Credit_Score, nameof(d.Credit_Score)),
		};
	}
}

public static ValidatorResult UniqueIdentifier(string input, string nameof)
{
	var result = new ValidatorResult();

	if (string.IsNullOrWhiteSpace(input))
	{
		result.AddFailure(nameof, "Unique identifier missing.");
		return result;
	}
	
	if (input.Length > 50)
		result.AddFailure(nameof, "Unique identifier exceeds maximum length.");
	
	return result;
}

public static ValidatorResult Date(string input, string nameof)
{
	var result = new ValidatorResult();

	if (string.IsNullOrWhiteSpace(input))
	{
		result.AddFailure(nameof, "Date missing.");
		return result;
	}

	if (!DateTime.TryParse(input, out _))
		result.AddFailure(nameof, $"Date invalid ({input}).");
	
	return result;
}

public static ValidatorResult CreditScore(string input, string nameof)
{
	var result = new ValidatorResult();

	if (string.IsNullOrWhiteSpace(input))
	{
		result.AddFailure(nameof, "Credit Score missing.");
		return result;
	}
	
	// TODO: Use Credit_Score_Source? For now, use least constraining range.
	if (!ValidationMethod.IndustrySpecificFicoCreditScore(input))
		result.AddFailure(nameof, "Credit Score out of expected range.");
	
	return result;
}

public class DemographicValidatorV2 : DemographicValidator
{
	public DemographicValidatorV2()
	{
	}
}

public class Demographic
{
	public string Customer_Id { get; set; }
	public string Branch_Id { get; set; }
	public string Effective_Date { get; set; }
	public string Credit_Score { get; set; }
	public string Credit_Score_Source { get; set; }
}

public class DemographicValidationResult : Demographic
{
	public bool IsValid { get; set; }
}