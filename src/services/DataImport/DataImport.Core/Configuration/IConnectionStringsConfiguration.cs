namespace DataImport.Core.Configuration
{
    public interface IConnectionStringsConfiguration
    {
        string QsRepositoryConnectionString { get; }
        string PartnerServiceBasePath { get; }
        string PartnersPath { get; }
    }
}