using Microsoft.Extensions.Configuration;

namespace Laso.DataImport.Core.Configuration
{
    public interface IConnectionStringConfiguration
    {
        string QsRepositoryConnectionString { get; }
        string LasoBlobStorageConnectionString { get; }
    }

    public class ConnectionStringConfiguration : IConnectionStringConfiguration
    {
        private readonly IConfiguration _appConfig;

        public ConnectionStringConfiguration(IConfiguration appConfig)
        {
            _appConfig = appConfig;
        }

        public string QsRepositoryConnectionString => _appConfig["ConnectionStrings:QsRepositoryConnectionString"];
        public string LasoBlobStorageConnectionString => _appConfig["ConnectionStrings:LasoBlobStorageConnectionString"];
    }
}
