using Laso.Insights.FunctionalTests.Utils;

namespace Laso.Insights.FunctionalTests.Services.DataPipeline
{
    public class DataQualityParts
    {
        public DataQualityParts() { }
        public DataQualityParts(Manifest manifest,Csv csvBaseline)
        {
            expectedManifest = manifest;
            Csv = csvBaseline;
        }


        public Manifest expectedManifest { get; set; }
        public Csv Csv { get; set; }
    }

    public class Csv
    {
        public Csv()
        {
        }

        public Csv(string blobCsvName)
        {
            BlobCsvName = blobCsvName;
            Rows= new AzureBlobStgFactory().Create()
                .DownloadCsvFileFromAutomationStorage(blobCsvName).Result;
        }


        public Csv(string blobCsvName, string[] rows)
        {
            BlobCsvName = blobCsvName;
            Rows = rows;
        }

        public string BlobCsvName { get; set; }

        public string[] Rows { get; set; }
    }
}
