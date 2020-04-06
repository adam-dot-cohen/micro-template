using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Laso.Provisioning.Core;
using Microsoft.Extensions.Configuration;

namespace Laso.Provisioning.Infrastructure
{
    public class AzureApplicationSecrets : IApplicationSecrets
    {
        private readonly IConfiguration _configuration;

        public AzureApplicationSecrets(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> SetSecret(string name, string value, CancellationToken cancellationToken)
        {
            var keyClient = GetSecretClient();
            var response = await keyClient.SetSecretAsync(name, value, cancellationToken).ConfigureAwait(false);
            return response.Value.Properties.Version;
        }

        public Task<string> GetSecret(string name, CancellationToken cancellationToken)
        {
            return GetSecret(name, null, cancellationToken);
        }

        public async Task<string> GetSecret(string name, string version, CancellationToken cancellationToken)
        {
            var keyClient = GetSecretClient();
            var response = await keyClient.GetSecretAsync(name, version, cancellationToken);
            return response.Value.Value;
        }

        private SecretClient GetSecretClient()
        {
            var vaultBaseUri = new Uri(_configuration["AzureKeyVault:VaultBaseUrl"]);
            var credential = new DefaultAzureCredential();
            var secretClient = new SecretClient(vaultBaseUri, credential);

            return secretClient;
        }
    }
}
