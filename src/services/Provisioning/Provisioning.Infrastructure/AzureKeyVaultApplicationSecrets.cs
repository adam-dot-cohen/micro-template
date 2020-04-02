using System.Threading;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;
using Laso.Provisioning.Core;

namespace Laso.Provisioning.Infrastructure
{
    public class AzureKeyVaultApplicationSecrets : IApplicationSecrets
    {
        private readonly SecretClient _client;

        public AzureKeyVaultApplicationSecrets(SecretClient client)
        {
            _client = client;
        }

        public async Task<string> SetSecret(string name, string value, CancellationToken cancellationToken)
        {
            var response = await _client.SetSecretAsync(name, value, cancellationToken).ConfigureAwait(false);
            return response.Value.Properties.Version;
        }

        public Task<string> GetSecret(string name, CancellationToken cancellationToken)
        {
            return GetSecret(name, null, cancellationToken);
        }

        public async Task<string> GetSecret(string name, string version, CancellationToken cancellationToken)
        {
            var response = await _client.GetSecretAsync(name, version, cancellationToken);
            return response.Value.Value;
        }
    }
}
