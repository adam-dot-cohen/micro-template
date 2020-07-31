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
  <Namespace>QS.Core.Infrastructure</Namespace>
  <Namespace>Infrastructure.IoC</Namespace>
  <Namespace>Infrastructure.App_start</Namespace>
  <Namespace>QS.Core.Domain.Entities</Namespace>
  <Namespace>QS.Core.Infrastructure.IO</Namespace>
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
var accounts = repository.GetAll<Business>()
	.Where(b => b.Type.HasFlag(BusinessType.Borrower) && b.User.BankAccounts.Any(a => a.AggregationBankAccounts.Any()))
	.SelectMany(b => b.User.BankAccounts
		.SelectMany(a => a.AggregationBankAccounts
			.SelectMany(ba => ba.Transactions.Select(t => new
			{
				Transaction_Id = t.Id,
				Account_Id = ba.Id,
				t.AvailableDate,
				t.PostedDate,
				t.CategoryValue,
				t.Amount,
				t.Memo,
				t.RunningBalance
			}))))
	//.Take(10)
	.ToList()
	.Select(a => new 
	{
		Transaction_Id = a.Transaction_Id.ToString("N").ToUpper(),
		Account_Id = a.Account_Id.ToString("N").ToUpper(),
		Transaction_Date = a.AvailableDate,
		Post_Date = a.PostedDate,
		Transaction_Category = BankAccountTransactionCategory.FromValue(a.CategoryValue).DisplayName,
		Amount = a.Amount,
		Memo_Field = a.Memo,
		MCC_Code = (string)null, // NONE
		Balance_After_Transaction = a.RunningBalance
	})
	.ToList()
	//.Dump(toDataGrid: true)
	;

// Generate
var now = SystemTime.UtcNow();
var dataOutputPath = $"{outputDirectory}{customerName}_Laso_R_AccountTransaction_{schemaVersion}_{asOfDate:yyyyMMdd}_{now:yyyyMMddHHmmss}.csv";

Directory.CreateDirectory(outputDirectory);
using (var outputStream = File.Create(dataOutputPath))
{
	var delimitedFileWriter = container.GetInstance<IDelimitedFileWriter>();
	delimitedFileWriter.Open(outputStream);

	delimitedFileWriter.WriteRecords(accounts);
}

// Encrypt
if (shouldEncrypt)
{
	//var encryption = container.GetInstance<IPgpEncryption>();
	//var encryptionKeyName = $"{customerName}-laso-pgp-public-key";

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

	//var blobStorageService = container.GetInstance<AzureBlobStorageService>();
	//blobStorageService.SaveFile($"{customerName}-incoming", encryptedDataOutputFilename, encryptedDataOutputFilename, encryptedFileBytes);
}