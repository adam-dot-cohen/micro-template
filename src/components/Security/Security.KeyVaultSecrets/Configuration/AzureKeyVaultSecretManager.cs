using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace Laso.Security.KeyVaultSecrets.Configuration
{
    public class AzureKeyVaultSecretManager
    {
        internal static AzureKeyVaultSecretManager Instance { get; } = new AzureKeyVaultSecretManager();

        public virtual string GetKey(KeyVaultSecret secret)
        {
            return secret.Name.Replace("--", ConfigurationPath.KeyDelimiter);
        }

        /// <summary>
        /// Maps secret to a configuration key.
        /// </summary>
        /// <param name="secret">The <see cref="KeyVaultSecret"/> instance.</param>
        /// <returns>Configuration key name to store secret value.</returns>
        public virtual bool Load(SecretProperties secret)
        {
            return true;
        }
    }
}
