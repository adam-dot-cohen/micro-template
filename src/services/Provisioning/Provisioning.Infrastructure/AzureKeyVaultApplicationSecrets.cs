using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure;
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

        public Task<bool> SecretExists(string name, CancellationToken cancellationToken)
        {
            return SecretExists(name, null, cancellationToken);
        }

        //TODO: alter this to use secret versions (if any version exists the secret exists)
        public async Task<bool> SecretExists(string name, string version, CancellationToken cancellationToken)
        {
            var secretExists = true;

            try
            {
                await GetSecret(name, version, cancellationToken);
            }
            catch (RequestFailedException e)
            {
                if (e.Status != (int)HttpStatusCode.NotFound)
                    throw;

                secretExists = false;
            }

            return secretExists;
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

        public Task DeleteSecret(string name, CancellationToken cancellationToken)
        {
            return _client.StartDeleteSecretAsync(name, cancellationToken);
        }
    }
}
