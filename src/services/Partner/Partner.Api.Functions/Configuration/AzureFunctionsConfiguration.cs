using System;
using Partner.Core.Configuration;

namespace Partner.Api.Functions.Configuration
{
    public class AzureFunctionsConfiguration : IApplicationConfiguration
    {
        public string QsRepositoryConnectionString => Environment.GetEnvironmentVariable("QsRepositoryConnectionString");
        public string BlobStorageConnectionString => Environment.GetEnvironmentVariable("BlobStorageConnectionString");
    }
}
