using Partner.Core.Configuration;
using System;

namespace Partner.Api.Functions.Configuration
{
    public class BlobStorageConfiguration : IBlobStorageConfiguration
    {
        public string LasoBlobStorageConnectionString => Environment.GetEnvironmentVariable("BlobStorage:LasoBlobStorageConnectionString");
    }
}
