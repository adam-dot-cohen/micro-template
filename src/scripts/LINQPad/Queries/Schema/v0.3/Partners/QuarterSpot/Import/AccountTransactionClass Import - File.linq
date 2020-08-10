<Query Kind="Statements">
  <Reference Relative="..\..\..\..\..\..\..\..\services\Catalog\Catalog.Domain\bin\Debug\netstandard2.1\Laso.Catalog.Domain.dll">E:\Work\Repos\Insights\src\services\Catalog\Catalog.Domain\bin\Debug\netstandard2.1\Laso.Catalog.Domain.dll</Reference>
  <Reference Relative="..\..\..\..\..\..\..\..\components\IO\IO\bin\Debug\netstandard2.1\Laso.IO.dll">E:\Work\Repos\Insights\src\components\IO\IO\bin\Debug\netstandard2.1\Laso.IO.dll</Reference>
  <Reference Relative="..\..\..\..\..\..\..\..\..\..\Platform\src\QS.Admin\bin\QS.Core.Base.dll">E:\Work\Repos\Platform\src\QS.Admin\bin\QS.Core.Base.dll</Reference>
  <Reference Relative="..\..\..\..\..\..\..\..\..\..\Platform\src\QS.Admin\bin\QS.Core.dll">E:\Work\Repos\Platform\src\QS.Admin\bin\QS.Core.dll</Reference>
  <Namespace>Laso.Catalog.Domain.FileSchema</Namespace>
  <Namespace>Laso.Catalog.Domain.FileSchema.Output</Namespace>
  <Namespace>Laso.IO.Structured</Namespace>
  <Namespace>QS.Core.Domain.Entities</Namespace>
  <Namespace>Laso.Catalog.Domain.FileSchema.Input</Namespace>
  <CopyLocal>true</CopyLocal>
</Query>

var customerName = "QuarterSpot";
var inputDirectory = $@"E:\Work\Insights\Partners\{customerName}\Data\";

var accountTransactionClassFilename = "Laso_QuarterSpot_R_AccountTransactionClass_v0.3_20200806_20200806142154.csv";
var accountTransactionClassStream = File.OpenRead($"{inputDirectory}{accountTransactionClassFilename}");

var accountTransactionFilename = "QuarterSpot_Laso_R_AccountTransaction_v0.3_20200806_20200806140715_SingleCustomer.csv";
var accountTransactionStream = File.OpenRead($"{inputDirectory}{accountTransactionFilename}");

// Read
var delimitedFileReader = new DelimitedFileReader();
delimitedFileReader.Open(accountTransactionClassStream);
var transactionClasses = delimitedFileReader.ReadRecords<AccountTransactionClass_v0_1>()
	.OrderBy(c => Guid.NewGuid())
	.ToList();

var accountTransactionFileReader = new DelimitedFileReader();
accountTransactionFileReader.Open(accountTransactionStream);
var transactions = accountTransactionFileReader.ReadRecords<AccountTransaction_v0_3>()
	.OrderBy(t => t.Transaction_Id)
	.ToList();

// Aggregate
var accountTransactionClasses = transactionClasses
	//.Take(100)
	.Select(t => new
	{
		Transaction = transactions.Single(x => x.Transaction_Id == t.Transaction_Id),
		Transaction_Class = t,
	})
	.Select(a => new
	{
		a.Transaction.Transaction_Id,
		a.Transaction_Class.Class,
		ClassName = BankAccountTransactionCategory.FromValue((int)a.Transaction_Class.Class).DisplayName,
		a.Transaction.Memo_Field,
		Amount = $"{a.Transaction.Amount:C}"
	})
	.ToList();

accountTransactionClasses.Dump();
