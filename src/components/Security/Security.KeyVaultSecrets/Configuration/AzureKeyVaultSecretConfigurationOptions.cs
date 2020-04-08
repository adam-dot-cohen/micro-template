using Azure.Security.KeyVault.Secrets;

namespace Laso.Security.KeyVaultSecrets.Configuration
{
    public class AzureKeyVaultSecretConfigurationOptions
    {
        public SecretClient Client { get; set; }
        public AzureKeyVaultSecretManager Manager { get; set; }
    }
}
