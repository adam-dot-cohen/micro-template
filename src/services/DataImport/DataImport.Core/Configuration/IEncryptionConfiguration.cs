using Microsoft.Extensions.Configuration;

namespace Laso.DataImport.Core.Configuration
{
    public interface IEncryptionConfiguration
    {
        string QuarterSpotPgpPublicKeyVaultName { get; }
        string QuarterSpotPgpPrivateKeyVaultName { get; }
        string QuarterSpotPgpPrivateKeyPassPhraseVaultName { get; }
    }

    // we only store vault names here (as opposed to KeyVault references using the KV config provider)
    // as we may want to grab versioned keys at some point.
    public class EncryptionConfiguration : IEncryptionConfiguration
    {
        private readonly IConfiguration _config;

        public EncryptionConfiguration(IConfiguration config)
        {
            _config = config;
        }

        public string QuarterSpotPgpPublicKeyVaultName => _config["EncryptionConfiguration:QuarterSpotPgpPublicKeyVaultName"];
        public string QuarterSpotPgpPrivateKeyVaultName => _config["EncryptionConfiguration:QuarterSpotPgpPrivateKeyVaultName"];
        public string QuarterSpotPgpPrivateKeyPassPhraseVaultName => _config["EncryptionConfiguration:QuarterSpotPgpPrivateKeyPassPhraseVaultName"];
    }
}
