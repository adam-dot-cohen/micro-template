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
  <Namespace>Infrastructure.Azure</Namespace>
  <Namespace>Infrastructure.IoC</Namespace>
  <Namespace>QS.Core.Domain</Namespace>
  <Namespace>QS.Core.Domain.Entities</Namespace>
  <Namespace>QS.Core.Domain.Enumerations</Namespace>
  <Namespace>QS.Core.Extensions</Namespace>
  <Namespace>QS.Core.Infrastructure</Namespace>
  <Namespace>QS.Core.Infrastructure.Azure</Namespace>
  <Namespace>QS.Core.Infrastructure.IO</Namespace>
  <Namespace>QS.Core.Infrastructure.Security.Cryptography</Namespace>
  <Namespace>Infrastructure.Storage.Blob</Namespace>
  <CopyLocal>true</CopyLocal>
</Query>

var customerName = "QuarterSpot";
var decryptionKeyName = $"{customerName.ToLower()}-laso-pgp-private-key";
var decryptionPassPhraseKeyName = $"{customerName.ToLower()}-laso-pgp-passphrase";

var container = IoC.Initialize();
var repository = container.GetInstance<IReadOnlyRepository>();
var blobStorageService = container.GetInstance<AzureBlobStorageService>();
var keyVaultService = container.GetInstance<IBackEndKeyVaultService>();
var encryption = container.GetInstance<IPgpEncryption>();

// Download
var encryptedDataInputFilename = "QuarterSpot_Laso_R_LoanApplication_20191025_20191025175150.csv.gpg";
var encryptedStream = blobStorageService.OpenRead($"{customerName}-incoming", encryptedDataInputFilename);
encryptedStream.ReadSeekBuffer();

// Decrypt
string decryptedData;
using (var decryptedStream = new MemoryStream())
{
	encryption.Decrypt(encryptedStream, decryptionKeyName, decryptionPassPhraseKeyName);
	encryptedStream.Stream.CopyTo(decryptedStream);
	
	//streamStack.Stream.CopyTo(decryptedOutputStream);
	decryptedStream.Seek(0, SeekOrigin.Begin);
	decryptedData = decryptedStream.GetString();
}

// Transform
var delimitedFileContentsStream = new MemoryStream(Encoding.UTF8.GetBytes(decryptedData));
var delimitedFileReader = container.GetInstance<IDelimitedFileReader>();
delimitedFileReader.Open(delimitedFileContentsStream);

var applications = delimitedFileReader.ReadRecords(new
{
	Loan_Application_Id = (string)null,
	Loan_Business_Id = (string)null,
	Loan_Customer_Id = (string)null,
	Loan_Account_Id = (string)null,
	Effective_Date = (DateTime?)null,
	Application_Date = (DateTime?)null,
	Product_Type = (string)null,
	Decision_Date = (DateTime?)null,
	Decision_Result = (string)null,
	Decline_Reason = (string)null,
	Application_Status = (string)null,
	Requested_Amount = (string)null,
	Approved_Term = (string)null,
	Approved_Installment = (string)null,
	Approved_Installment_Frequency = (string)null,
	Approved_Amount = (string)null,
	Approved_Interest_Rate = (string)null,
	Approved_Interest_Rate_Method = (string)null,
	Accepted_Term = (string)null,
	Accepted_Installment = (string)null,
	Accepted_Installment_Frequency = (string)null,
	Accepted_Amount = (string)null,
	Accepted_Interest_Rate = (string)null,
	Accepted_Interest_Rate_Method = (string)null
});

applications.Dump(toDataGrid: true);