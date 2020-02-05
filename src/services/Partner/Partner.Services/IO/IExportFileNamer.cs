using System;
using System.Collections.Generic;
using System.Text;
using Partner.Domain.Laso.Common;

namespace Partner.Services.IO
{
    public interface IExportFileNamer
    {
        string GetName(PartnerIdentifier exporter, PartnerIdentifier importer);
    }
}
