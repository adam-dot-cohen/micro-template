<Query Kind="Statements">
  <Reference Relative="..\..\..\..\..\..\..\..\services\Catalog\Catalog.Domain\bin\Debug\netstandard2.1\Laso.Catalog.Domain.dll">E:\Work\Repos\Insights\src\services\Catalog\Catalog.Domain\bin\Debug\netstandard2.1\Laso.Catalog.Domain.dll</Reference>
  <Reference Relative="..\..\..\..\..\..\..\..\components\IO\IO\bin\Debug\netstandard2.1\Laso.IO.dll">E:\Work\Repos\Insights\src\components\IO\IO\bin\Debug\netstandard2.1\Laso.IO.dll</Reference>
  <Namespace>Laso.IO.Structured</Namespace>
  <Namespace>Laso.Catalog.Domain.FileSchema</Namespace>
  <CopyLocal>true</CopyLocal>
</Query>

var customerName = "QuarterSpot";
var inputDirectory = $@"E:\Work\Insights\Partners\{customerName}\Data\";
var inputFilename = "QuarterSpot_Laso_R_Account_v0.3_20200731_20200731171915.csv";

var inputStream = File.OpenRead($"{inputDirectory}{inputFilename}");

// Transform
var delimitedFileReader = new DelimitedFileReader();
delimitedFileReader.Open(inputStream);

var accounts = delimitedFileReader.ReadRecords<Account_v0_3>();

accounts.Dump(true);