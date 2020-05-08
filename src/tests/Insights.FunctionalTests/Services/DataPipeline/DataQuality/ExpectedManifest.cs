using System;
using System.Collections.Generic;
using System.Text;

namespace Laso.Insights.FunctionalTests.Services.DataPipeline.DataQuality
{
    public class ExpectedMetrics
    {
 
        public Metrics GetTestCsvAllCuratedExpectedMetrics(int rows = 2)
        {
            return new Metrics
            {
                adjustedBoundaryRows = 0,
                curatedRows = rows,
                quality = 2,
                rejectedCSVRows = 0,
                rejectedConstraintRows = 0,
                rejectedSchemaRows = 0,
                sourceRows = rows
            };
        }
        public Metrics GetTestCsvAllRejectedExpectedMetrics(int rows = 2)
        {
            return new Metrics
            {
                adjustedBoundaryRows = 0,
                curatedRows = 0,
                quality = 0,
                rejectedCSVRows = rows,
                rejectedConstraintRows = 0,
                rejectedSchemaRows = 0,
                sourceRows = rows
            };
        }
    }

    public class ExpectedManifest
    {
        protected const string _raw = "raw";
        protected const string _cold = "cold";
        protected const string _storage = "storage";
        protected const string _archive = "archive";
        public string Category = "";

        public Manifest GetExpectedManifest(Category category, Storage storageType, Metrics metrics = null)
        {
            var manifest = new Manifest();

            switch (storageType)
            {
                case Storage.cold:
                    manifest.type = _archive;
                    break;
                default:
                    manifest.type = storageType.ToString();
                    break;
            }

            Category = category.ToString();
            var documentsItem = new DocumentsItem();

            if (metrics == null)
                metrics = new Metrics
                {
                    adjustedBoundaryRows = 0,
                    curatedRows = 0,
                    quality = 0,
                    rejectedCSVRows = 0,
                    rejectedConstraintRows = 0,
                    rejectedSchemaRows = 0,
                    sourceRows = 0
                };

            documentsItem.metrics = metrics;

            documentsItem.dataCategory = Category;

            manifest.tenantId = GlobalSetUp.AutomationPartnerId;
            manifest.tenantName = GlobalSetUp.AutomationPartnerName;
            manifest.documents = new List<DocumentsItem> {documentsItem};

            return manifest;
        }
    }

}
