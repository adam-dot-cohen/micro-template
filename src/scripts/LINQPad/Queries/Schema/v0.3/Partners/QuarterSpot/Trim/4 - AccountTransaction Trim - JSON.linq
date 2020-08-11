<Query Kind="Statements">
  <Reference Relative="..\..\..\..\..\..\..\..\services\Catalog\Catalog.Domain\bin\Debug\netstandard2.1\Laso.Catalog.Domain.dll">E:\Work\Repos\Insights\src\services\Catalog\Catalog.Domain\bin\Debug\netstandard2.1\Laso.Catalog.Domain.dll</Reference>
  <Reference Relative="..\..\..\..\..\..\..\..\components\IO\IO\bin\Debug\netstandard2.1\Laso.IO.dll">E:\Work\Repos\Insights\src\components\IO\IO\bin\Debug\netstandard2.1\Laso.IO.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\netstandard.dll</Reference>
  <Namespace>Laso.Catalog.Domain.FileSchema</Namespace>
  <Namespace>Laso.Catalog.Domain.FileSchema.Input</Namespace>
  <Namespace>Laso.IO.Structured</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <CopyLocal>true</CopyLocal>
</Query>

var customerName = "QuarterSpot";
var asOfDate = DateTime.UtcNow.Date;
var inputDirectory = $@"E:\Work\Insights\Partners\{customerName}\Data\";
var inputFilename = "QuarterSpot_Laso_R_AccountTransaction_v0.3_20200731_20200801012047.csv";
var outputDirectory = $@"E:\Work\Insights\Partners\{customerName}\Data\";

var filterIds = new[]
{
	"1B667EB49F264EC5B49253C0799918E2",
	//"DB36AF09904D4142BC0B06338A560BCC",
	//"9A00210241CE472F99A730192DFC7F1A",
	//"4290A6EF733945A4B631179DAB37C41A",
	//"79B75AE447DA4AD2AC669C787333861C",
	//"D69E1E74E59946A6B8590126D0367BA5",
	//"66E3BA57CE8245B6A14F687E219893D1",
	//"E8DA2B9C49D7404FBAADEB7BCC12BE6E",
	//"8E05193A29B145BCB73E2DF5FAACA3B2",
	//"B66412ABCD0F49D99BE6ABC24D5D9514"
};

// Read
using var inputStream = File.OpenRead($"{inputDirectory}{inputFilename}");
using var delimitedFileReader = new DelimitedFileReader();
delimitedFileReader.Open(inputStream);

var filteredTransactions = delimitedFileReader
	.ReadRecords<AccountTransaction_v0_3>()
	.Where(a => filterIds.Contains(a.Account_Id))
	.ToList();

// Write
var now = DateTime.UtcNow;
var schemaVersion = "v0.3";
var outputFilename = $"{outputDirectory}{customerName}_Laso_R_AccountTransaction_{schemaVersion}_{asOfDate:yyyyMMdd}_{now:yyyyMMddHHmmss}.json";

var output = JsonSerializer.SerializeToUtf8Bytes<IEnumerable<AccountTransaction_v0_3>>(
	filteredTransactions,
	new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = true });
	
File.WriteAllBytesAsync(outputFilename, output);
