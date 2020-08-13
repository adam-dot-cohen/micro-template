<Query Kind="Statements">
  <Reference Relative="..\..\..\..\..\..\..\..\services\Catalog\Catalog.Domain\bin\Debug\netstandard2.1\Laso.Catalog.Domain.dll">E:\Work\Repos\Insights\src\services\Catalog\Catalog.Domain\bin\Debug\netstandard2.1\Laso.Catalog.Domain.dll</Reference>
  <Reference Relative="..\..\..\..\..\..\..\..\components\IO\IO\bin\Debug\netstandard2.1\Laso.IO.dll">E:\Work\Repos\Insights\src\components\IO\IO\bin\Debug\netstandard2.1\Laso.IO.dll</Reference>
  <Namespace>Laso.IO.Structured</Namespace>
  <Namespace>Laso.Catalog.Domain.FileSchema</Namespace>
  <CopyLocal>true</CopyLocal>
</Query>

var customerName = "QuarterSpot";
var inputDirectory = $@"E:\Work\Insights\Partners\{customerName}\Data\";
var inputFilename = "QuarterSpot_Laso_R_AccountTransaction_v0.3_20200731_20200801012047.csv";

var inputStream = File.OpenRead($"{inputDirectory}{inputFilename}");

// Read
var delimitedFileReader = new DelimitedFileReader();
delimitedFileReader.Open(inputStream);

var transactions = delimitedFileReader.ReadRecords<AccountTransaction_v0_3>();

var accountTransactions = transactions
	.GroupBy(a => a.Account_Id)
	.Select(g => new { g.Key, Count = g.Count() })
	.OrderByDescending(a => a.Count)
	.ToList();

accountTransactions.Dump(true);
