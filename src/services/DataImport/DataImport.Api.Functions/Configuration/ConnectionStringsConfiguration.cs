using System;
using DataImport.Core.Configuration;

namespace DataImport.Api.Functions.Configuration
{
    // MS recommends that you do not use the ConnectionStrings section for function binding as it is for frameworks only.
    // https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows    
    public class ConnectionStringsConfiguration : IConnectionStringsConfiguration
    {
        public string QsRepositoryConnectionString => Environment.GetEnvironmentVariable("QsRepositoryConnectionString");
        public string PartnerServiceBasePath => Environment.GetEnvironmentVariable("PartnerServiceBasePath");
        public string PartnersResourcePath => Environment.GetEnvironmentVariable("PartnersPath");

        public string SubscriptionsServiceBasePath => Environment.GetEnvironmentVariable("SubscriptionsServiceBasePath");

        public string SubscriptionsResourcePath => Environment.GetEnvironmentVariable("SubscriptionsPath");
        public string ImportHistoryServiceBasePath => Environment.GetEnvironmentVariable("ImportHistoryServiceBasePath");
        public string ImportHistoryResourcePath => Environment.GetEnvironmentVariable("ImportHistoryResourcePath");
    }
}
