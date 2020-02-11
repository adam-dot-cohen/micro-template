namespace DataImport.Core.Configuration
{
    public interface IConnectionStringsConfiguration
    {
        string QsRepositoryConnectionString { get; }
        string PartnerServiceBasePath { get; }
        string PartnersResourcePath { get; }
        string SubscriptionsServiceBasePath { get; }
        string SubscriptionsResourcePath { get; }
    }
}