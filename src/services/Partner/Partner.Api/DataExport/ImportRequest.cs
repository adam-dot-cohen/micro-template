using System;
using Partner.Domain.Laso.Common;
using Partner.Services.DataImport;

namespace Partner.Api.DataImport
{
    public class ImportRequest
    {
        public PartnerIdentifier ExportFrom { get; set; }
        public PartnerIdentifier ImportTo { get; set; }
        public ImportType[] Imports { get; set; }
        public ImportFrequency Frequency { get; set; }
    }
}
