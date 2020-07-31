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
  <Reference Relative="..\..\..\..\..\..\..\..\..\..\Platform\src\QS.Admin\bin\QS.Core.dll">E:\Work\Repos\Platform\src\QS.Admin\bin\QS.Core.dll</Reference>
  <Reference Relative="..\..\..\..\..\..\..\..\..\..\Platform\src\QS.Admin\bin\QS.Core.Storage.dll">E:\Work\Repos\Platform\src\QS.Admin\bin\QS.Core.Storage.dll</Reference>
  <Reference Relative="..\..\..\..\..\..\..\..\..\..\Platform\src\QS.Admin\bin\StructureMap.dll">E:\Work\Repos\Platform\src\QS.Admin\bin\StructureMap.dll</Reference>
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

var hashSalt = Guid.NewGuid().ToString("N").ToUpper();
var hash = new SHA1Managed();

// Query
var schemaVersion = "v0.3";
var demographics = repository.GetAll<Business>()
	.Where(b => b.Type.HasFlag(BusinessType.Borrower))
	.SelectMany(b => b.Leads
		.SelectMany(l => l.Principals
			.Where(p => (p.CreditScore != null) && (p.SsnEncrypted != null))
			.Select(p => new
			{
				Ssn_Encrypted = p.SsnEncrypted,
				CreditScore_Metrics = p.EvaluationMetrics.Where(m => m.Type.Id == EvaluationMetricTypeValue.CreditScore.Value),
				CreditScore_Manual = p.CreditScore,
				CreditScore_Manual_EffectiveDate = l.Created
			})))
	//.Take(10)
	.ToList()
	.Select(a => new 
	{
		Customer_Id = BitConverter.ToString(hash.ComputeHash(new MemoryStream(Encoding.UTF8.GetBytes(
				NormalizationMethod.TaxId(EncryptedString.FromEncryptedValue(a.Ssn_Encrypted).Value + hashSalt)))))
			.Replace("-", ""),
		Branch_Id = (string)null, // NONE
		Effective_Date = $"{a.CreditScore_Metrics.FirstOrDefault()?.EffectiveTime.Date ?? a.CreditScore_Manual_EffectiveDate.Date:M/d/yyyy}",
		Credit_Score = (a.CreditScore_Metrics.FirstOrDefault()?.Value.ConvertTo<decimal>() ?? a.CreditScore_Manual)?.RoundNatural(0)
	})
	.ToList()
//	.Dump(toDataGrid: true)
	;

// Generate
Directory.CreateDirectory(outputDirectory);

var now = SystemTime.UtcNow();

var saltOutputFilename = $"{outputDirectory}{customerName}_Laso_R_Demographic_{schemaVersion}_{asOfDate:yyyyMMdd}_{now:yyyyMMddHHmmss}.salt";
File.WriteAllText(saltOutputFilename, hashSalt);

var dataOutputFilename = $"{outputDirectory}{customerName}_Laso_R_Demographic_{schemaVersion}_{asOfDate:yyyyMMdd}_{now:yyyyMMddHHmmss}.csv";

using (var outputStream = File.Create(dataOutputFilename))
{
	var delimitedFileWriter = container.GetInstance<IDelimitedFileWriter>();
	delimitedFileWriter.Open(outputStream);

	delimitedFileWriter.WriteRecords(demographics);
}

// Encrypt
if (shouldEncrypt)
{
	//var encryption = container.GetInstance<IPgpEncryption>();
	//var encryptionKeyName = $"{customerName.ToLower()}-laso-pgp-public-key";

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

	//var blobStorageService = container.GetInstance<AzureBlobStorageService>();
	//blobStorageService.SaveFile($"{customerName}-incoming", encryptedDataOutputFilename, encryptedDataOutputFilename, encryptedFileBytes);
}