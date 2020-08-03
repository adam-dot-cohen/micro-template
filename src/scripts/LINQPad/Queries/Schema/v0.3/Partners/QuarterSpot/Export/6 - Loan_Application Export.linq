<Query Kind="Statements">
  <Connection>
    <ID>d06d070d-6f62-4208-83b8-4b29f98a9d14</ID>
    <Driver>EntityFrameworkDbContext</Driver>
    <CustomAssemblyPath>E:\Work\Repos\Platform\src\QS.Admin\bin\Infrastructure.dll</CustomAssemblyPath>
    <CustomTypeName>Infrastructure.Data.QsContext</CustomTypeName>
    <AppConfigPath>E:\Work\Scripts\LinqPad\Connections\Web.Production - Read Only.config</AppConfigPath>
    <DisplayName>Production - Read Only</DisplayName>
    <IsProduction>true</IsProduction>
  </Connection>
  <Reference Relative="..\..\..\..\..\..\..\..\..\..\Platform\src\QS.Admin\bin\EntityFramework.dll">E:\Work\Repos\Platform\src\QS.Admin\bin\EntityFramework.dll</Reference>
  <Reference Relative="..\..\..\..\..\..\..\..\..\..\Platform\src\QS.Admin\bin\EntityFramework.SqlServer.dll">E:\Work\Repos\Platform\src\QS.Admin\bin\EntityFramework.SqlServer.dll</Reference>
  <Reference Relative="..\..\..\..\..\..\..\..\..\..\Platform\src\QS.Admin\bin\Infrastructure.dll">E:\Work\Repos\Platform\src\QS.Admin\bin\Infrastructure.dll</Reference>
  <Reference Relative="..\..\..\..\..\..\..\..\..\..\Platform\src\QS.Admin\bin\Infrastructure.Storage.dll">E:\Work\Repos\Platform\src\QS.Admin\bin\Infrastructure.Storage.dll</Reference>
  <Reference Relative="..\..\..\..\..\..\..\..\..\..\Platform\src\QS.Admin\bin\QS.Core.Base.dll">E:\Work\Repos\Platform\src\QS.Admin\bin\QS.Core.Base.dll</Reference>
  <Reference Relative="..\..\..\..\..\..\..\..\..\..\Platform\src\QS.Admin\bin\QS.Core.dll">E:\Work\Repos\Platform\src\QS.Admin\bin\QS.Core.dll</Reference>
  <Reference Relative="..\..\..\..\..\..\..\..\..\..\Platform\src\QS.Admin\bin\QS.Core.Storage.dll">E:\Work\Repos\Platform\src\QS.Admin\bin\QS.Core.Storage.dll</Reference>
  <Reference Relative="..\..\..\..\..\..\..\..\..\..\Platform\src\QS.Admin\bin\StructureMap.dll">E:\Work\Repos\Platform\src\QS.Admin\bin\StructureMap.dll</Reference>
  <Namespace>Infrastructure.Azure</Namespace>
  <Namespace>Infrastructure.IoC</Namespace>
  <Namespace>Infrastructure.Storage.Blob</Namespace>
  <Namespace>QS.Core.Domain</Namespace>
  <Namespace>QS.Core.Domain.Entities</Namespace>
  <Namespace>QS.Core.Domain.Enumerations</Namespace>
  <Namespace>QS.Core.Extensions</Namespace>
  <Namespace>QS.Core.Infrastructure</Namespace>
  <Namespace>QS.Core.Infrastructure.Azure</Namespace>
  <Namespace>QS.Core.Infrastructure.IO</Namespace>
  <Namespace>QS.Core.Infrastructure.Security.Cryptography</Namespace>
  <Namespace>QS.Core.IO.Streaming</Namespace>
  <CopyLocal>true</CopyLocal>
</Query>

var customerName = "QuarterSpot";
var outputDirectory = $@"E:\Work\Insights\Partners\{customerName}\Data\";
var asOfDate = SystemTime.UtcNow().Date;

var container = IoC.Initialize();
var repository = container.GetInstance<IReadOnlyRepository>();
var blobStorageService = container.GetInstance<AzureBlobStorageService>();
var keyVaultService = container.GetInstance<IBackEndKeyVaultService>();
var encryption = container.GetInstance<IPgpEncryption>();

// Query
var schemaVersion = "v0.3";
var applications = repository.GetAll<Lead>()
	.Where(l => l.Created <= asOfDate)
	.Select(l => new
	{
		l.Id,
		Business_Id = l.Business != null ? (long?)l.Business.Id : null,
		l.LoanGroupId,
		l.Created,
		Product_Name = l.Configuration.ProductChannelOffering.Product.Name,
		l.ReportingGroupValue,
		l.RequestedLoanAmount,
		l.OfferedTerms,
		l.AcceptedTerms,
		Declined_Reason = l.DeclinedReason.Name
	})
	.ToList()
	.Select(l => new
	{
		Loan_Application_Id = l.Id,
		Loan_Business_Id = l.Business_Id,
		Loan_Customer_Id = (Guid?)null,
		Loan_Account_Id = l.LoanGroupId,
		Effective_Date = asOfDate.Date,
		Application_Date = l.Created.Date,
		Product_Type = l.Product_Name,
		Decision_Date = (string)null, // ?
		Decision_Result = (string)null, // ?
		Decline_Reason = l.Declined_Reason, // ?
		Application_Status = LeadTaskReportingGroup.FromValue(l.ReportingGroupValue).DisplayName,
		Requested_Amount = l.RequestedLoanAmount.ToCurrency(),
		Approved_Term = l.OfferedTerms?.MaxTerm,
		Approved_Installment = (string)null, // ?
		Approved_Installment_Frequency = (string)null, // ?
		Approved_Amount = l.OfferedTerms?.MaxAmount.ToCurrency(),
		Approved_Interest_Rate = (string)null, // ?
		Approved_Interest_Rate_Method = (string)null, // ?
		Accepted_Term = l.AcceptedTerms?.TermInstallment?.Term.Name,
		Accepted_Installment = l.AcceptedTerms?.InstallmentAmount.ToCurrency(),
		Accepted_Installment_Frequency = l.AcceptedTerms?.TermInstallment?.Installment.DisplayName,
		Accepted_Amount = l.AcceptedTerms?.Principal.ToCurrency(),
		Accepted_Interest_Rate = l.AcceptedTerms?.InterestRate.ToPercentage(),
		Accepted_Interest_Rate_Method = "Full",
	})
	.ToList()
	.Dump(toDataGrid: true)
	;

// Generate
var now = SystemTime.UtcNow();
var dataOutputPath = $"{outputDirectory}{customerName}_Laso_R_LoanApplication_{schemaVersion}_{asOfDate:yyyyMMdd}_{now:yyyyMMddHHmmss}.csv";

Directory.CreateDirectory(outputDirectory);
using (var outputStream = File.Create(dataOutputPath))
{
	var delimitedFileWriter = container.GetInstance<IDelimitedFileWriter>();
	delimitedFileWriter.Open(outputStream);
	
	delimitedFileWriter.WriteRecords(applications);
}

//// Encrypt
//var encryptionKeyName = $"{customerName.ToLower()}-laso-pgp-public-key";
//var encryptedDataOutputPath = $"{dataOutputPath}.gpg";
//
//using (var inputStream = File.OpenRead(dataOutputPath))
//{
//	using (var encryptedOutputStream = File.Create(encryptedDataOutputPath))
//	{
//		using (var streamStack = new StreamStack(encryptedOutputStream))
//		{
//			encryption.Encrypt(streamStack, encryptionKeyName);
//			inputStream.CopyTo(streamStack.Stream);
//		}
//	}
//}
//
//// Upload
//var encryptedFileBytes = File.ReadAllBytes(encryptedDataOutputPath);
//var encryptedDataOutputFilename = Path.GetFileName(encryptedDataOutputPath);
//blobStorageService.SaveFile($"{customerName}-incoming", encryptedDataOutputFilename, encryptedDataOutputFilename, encryptedFileBytes);