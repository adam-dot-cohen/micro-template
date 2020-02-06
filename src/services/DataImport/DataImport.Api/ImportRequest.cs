using System;
using DataImport.Domain.Laso.Common;
using DataImport.Services.DataImport;

namespace DataImport.Api
{
    public class ImportRequest
    {
        public PartnerIdentifier ExportFrom { get; set; }
        public PartnerIdentifier ImportTo { get; set; }
        public ImportType[] Imports { get; set; }
        public ImportFrequency Frequency { get; set; }
    }
}
