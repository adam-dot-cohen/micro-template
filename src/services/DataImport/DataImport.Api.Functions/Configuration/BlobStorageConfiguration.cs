using System;
using DataImport.Core.Configuration;

namespace DataImport.Api.Functions.Configuration
{
    public class BlobStorageConfiguration : IBlobStorageConfiguration
    {
        public string LasoBlobStorageConnectionString => Environment.GetEnvironmentVariable("LasoBlobStorageConnectionString");
    }
}
