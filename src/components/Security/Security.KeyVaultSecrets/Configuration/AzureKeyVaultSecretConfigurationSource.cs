using Microsoft.Extensions.Configuration;

namespace Laso.Security.KeyVaultSecrets.Configuration
{
    public class AzureKeyVaultSecretConfigurationSource : IConfigurationSource
    {
        private readonly AzureKeyVaultSecretConfigurationOptions _options;

        public AzureKeyVaultSecretConfigurationSource(AzureKeyVaultSecretConfigurationOptions options)
        {
            _options = options;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new AzureKeyVaultSecretConfigurationProvider(_options.Client, _options.Manager);
        }
    }
}
