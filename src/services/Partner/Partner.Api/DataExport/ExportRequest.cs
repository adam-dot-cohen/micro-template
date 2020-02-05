using System;
using Partner.Domain.Laso.Common;
using Partner.Services.DataExport;

namespace Partner.Api.DataExport
{
    public class ExportRequest
    {
        public PartnerIdentifier Partner { get; set; }
        public ExportType[] Exports { get; set; }
    }
}
