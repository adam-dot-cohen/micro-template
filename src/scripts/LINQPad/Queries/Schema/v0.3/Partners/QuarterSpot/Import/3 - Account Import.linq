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
var encryptedDataInputFilename = "QuarterSpot_Laso_R_Account_20191025_20191025212654.csv.gpg";
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

var accounts = delimitedFileReader.ReadRecords(new
{
	Account_Id = (string)null,
	Business_Id = (string)null,
	Customer_Id = (string)null,
	Effective_Date = (DateTime?)null,
	Account_Type = (string)null,
	Interest_Rate_Method = (string)null,
	Interest_Rate = (string)null,
	Account_Open_Date = (DateTime?)null,
	Current_Balance = (string)null,
	Current_Balance_Date = (DateTime?)null,
	Average_Daily_Balance = (string)null,
	Account_Closed_Date = (DateTime?)null,
	Account_Closed_Reason = (string)null
});

accounts.Dump(toDataGrid: true);