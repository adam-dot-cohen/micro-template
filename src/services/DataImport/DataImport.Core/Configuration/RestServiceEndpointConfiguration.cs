namespace DataImport.Core.Configuration
{
    public class RestServiceEndpointConfiguration
    {
        public string PartnerServiceBasePath { get; set; }
        public string PartnersResourcePath { get; set; }
        public string SubscriptionsServiceBasePath { get; set; }
        public string SubscriptionsResourcePath { get; set; }
        public string ImportHistoryServiceBasePath { get; set; }
        public string ImportHistoryResourcePath { get; set; }
    }
}
