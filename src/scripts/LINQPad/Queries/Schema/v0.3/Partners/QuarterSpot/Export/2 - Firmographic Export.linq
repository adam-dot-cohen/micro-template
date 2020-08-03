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
  <Reference Relative="..\..\..\..\..\..\..\..\..\..\Platform\src\QS.Admin\bin\Infrastructure.dll">E:\Work\Repos\Platform\src\QS.Admin\bin\Infrastructure.dll</Reference>
  <Reference Relative="..\..\..\..\..\..\..\..\..\..\Platform\src\QS.Admin\bin\Infrastructure.Storage.dll">E:\Work\Repos\Platform\src\QS.Admin\bin\Infrastructure.Storage.dll</Reference>
  <Reference Relative="..\..\..\..\..\..\..\..\..\..\Platform\src\QS.Admin\bin\QS.Core.dll">E:\Work\Repos\Platform\src\QS.Admin\bin\QS.Core.dll</Reference>
  <Reference Relative="..\..\..\..\..\..\..\..\..\..\Platform\src\QS.Admin\bin\QS.Core.Storage.dll">E:\Work\Repos\Platform\src\QS.Admin\bin\QS.Core.Storage.dll</Reference>
  <Namespace>Infrastructure.App_start</Namespace>
  <Namespace>Infrastructure.Azure</Namespace>
  <Namespace>Infrastructure.IoC</Namespace>
  <Namespace>Infrastructure.Storage.Blob</Namespace>
  <Namespace>QS.Core.Domain</Namespace>
  <Namespace>QS.Core.Domain.Entities</Namespace>
  <Namespace>QS.Core.Domain.Entities.Metrics</Namespace>
  <Namespace>QS.Core.Domain.Enumerations</Namespace>
  <Namespace>QS.Core.Extensions</Namespace>
  <Namespace>QS.Core.Infrastructure</Namespace>
  <Namespace>QS.Core.Infrastructure.IO</Namespace>
  <Namespace>QS.Core.Infrastructure.Matching</Namespace>
  <Namespace>QS.Core.Infrastructure.Security.Cryptography</Namespace>
  <Namespace>QS.Core.IO.Streaming</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
  <CopyLocal>true</CopyLocal>
</Query>

var customerName = "QuarterSpot";
var outputDirectory = $@"E:\Work\Insights\Partners\{customerName}\Data\";
var shouldEncrypt = false;
var asOfDate = SystemTime.UtcNow().Date;

var container = IoC.Initialize();
container.GetInstance<EncryptedStringStartup>().Start();

var context = new QsContext();
(context as IObjectContextAdapter).ObjectContext.CommandTimeout = 3600;
var repository = new Repository(context);

// Query
var schemaVersion = "v0.3";
var demographics = repository.GetAll<Business>()
	.Where(b => b.Type.HasFlag(BusinessType.Borrower))
	.Select(b => new
	{
		Business_Id = b.Id,
		Date_Started = b.Established,
		Industry_Naics = b.Industry,
		Industry_Sic = b.Industry.SicCodes.FirstOrDefault(),
		Business_EntityType_Value = b.BusinessEntityTypeValue,
		Legal_Business_Name = b.LegalName,
		Business_Phone = b.Phone,
		EIN = b.TaxId,
		Postal_Code = b.Zip
	})
	//.Take(25)
	.ToList()
	.Select(a => new 
	{
		a.Business_Id,
		Customer_Id = (string)null, // ?
		Effective_Date = $"{asOfDate:M/d/yyyy}",
		Date_Started = $"{a.Date_Started:M/d/yyyy}",
		Industry_Naics = a.Industry_Naics?.Code,
		Industry_Sic = a.Industry_Sic?.Code,
		Business_Type = a.Business_EntityType_Value != null ? BusinessEntityType.FromValue(a.Business_EntityType_Value.Value).DisplayName : null,
		a.Legal_Business_Name,
		Business_Phone = NormalizationMethod.Phone10(a.Business_Phone),
		EIN = NormalizationMethod.TaxId(a.EIN),
		Postal_Code = NormalizationMethod.Zip5(a.Postal_Code)
	})
	.ToList()
	//.Dump(toDataGrid: true)
	;

// Generate
var now = SystemTime.UtcNow();
var dataOutputPath = $"{outputDirectory}{customerName}_Laso_R_Firmographic_{schemaVersion}_{asOfDate:yyyyMMdd}_{now:yyyyMMddHHmmss}.csv";

Directory.CreateDirectory(outputDirectory);
using (var outputStream = File.Create(dataOutputPath))
{
	var delimitedFileWriter = container.GetInstance<IDelimitedFileWriter>();
	delimitedFileWriter.Open(outputStream);

	delimitedFileWriter.WriteRecords(demographics);
}

// Encrypt
if (shouldEncrypt)
{
	var encryption = container.GetInstance<IPgpEncryption>();
	var encryptionKeyName = $"{customerName}-laso-pgp-public-key";
	
	//var encryptedDataOutputPath = $"{dataOutputPath}.gpg";
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
}