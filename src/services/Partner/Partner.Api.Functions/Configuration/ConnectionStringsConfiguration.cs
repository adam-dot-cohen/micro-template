using System;
using Partner.Core.Configuration;

namespace Partner.Api.Functions.Configuration
{
    // MS recommends that you do not use the ConnectionStrings section for function binding as it is for frameworks only.
    // https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows
    public class ConnectionStringsConfiguration : IConnectionStringsConfiguration
    {
        public string QsRepositoryConnectionString => Environment.GetEnvironmentVariable("QsRepositoryConnectionString");
    }
}
