namespace DataImport.Core.Configuration
{
    public interface IConnectionStringsConfiguration
    {
        string QsRepositoryConnectionString { get; }
        string PartnerServiceEndpoint { get; }
    }
}