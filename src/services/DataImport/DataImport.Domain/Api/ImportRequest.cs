﻿using System;

namespace DataImport.Domain.Api
{
    public class ImportRequest
    {
        public string ExportFromPartnerId { get; set; }        
        public ImportType[] Imports { get; set; }
        public ImportFrequency Frequency { get; set; }
    }
}
