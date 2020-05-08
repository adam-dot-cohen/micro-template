﻿using System;
using System.Collections.Generic;
using System.Text;
using Laso.Insights.FunctionalTests.Utils;

namespace Laso.Insights.FunctionalTests.Services.DataPipeline
{
    public class DataQualityParts
    {
        public DataQualityParts() { }
        public DataQualityParts(Manifest manifest,Csv csvBaseline)
        {
            this.expectedManifest = manifest;
            this.Csv = csvBaseline;
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
            this.BlobCsvName = blobCsvName;
            this.Rows=new AzureBlobStg()
                .DownloadCsvFileFromAutomationStorage(blobCsvName).Result;
        }

        
        public Csv(string blobCsvName, string[] rows)
        {
            this.BlobCsvName = blobCsvName;
            this.Rows = rows;
        }

        public string BlobCsvName { get; set; }

        public string[] Rows { get; set; }
    }
}
