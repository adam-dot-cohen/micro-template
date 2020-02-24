using Microsoft.Extensions.Configuration;

namespace Laso.DataImport.Core.Configuration
{
    public interface IAzureKeyVaultConfiguration
    {
        string ClientId { get; }
        string Secret { get; }
        string VaultBaseUrl { get; }
    }

    public class AzureKeyVaultConfiguration : IAzureKeyVaultConfiguration
    {
        private readonly IConfiguration _appConfig;

        public AzureKeyVaultConfiguration(IConfiguration appConfig)
        {
            _appConfig = appConfig;
        }

        public string ClientId => _appConfig["AzureKeyVault:ClientId"];
        public string Secret => _appConfig["AzureKeyVault:Secret"];
        public string VaultBaseUrl => _appConfig["AzureKeyVault:VaultBaseUrl"];
    }
}
