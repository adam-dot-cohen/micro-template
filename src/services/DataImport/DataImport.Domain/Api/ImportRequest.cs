using System;
using DataImport.Domain.Api.Common;

namespace DataImport.Domain.Api
{
    public class ImportRequest
    {
        public PartnerIdentifier ExportFrom { get; set; }
        public PartnerIdentifier ImportTo { get; set; }
        public ImportType[] Imports { get; set; }
        public ImportFrequency Frequency { get; set; }
    }
}
