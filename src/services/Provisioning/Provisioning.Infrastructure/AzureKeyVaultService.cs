using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Laso.Provisioning.Core;
using Microsoft.Extensions.Configuration;

namespace Laso.Provisioning.Infrastructure
{
    public class AzureKeyVaultService : IKeyVaultService
    {
        private readonly IConfiguration _configuration;

        public AzureKeyVaultService(IConfiguration configuration)
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
            var vaultUri = new Uri(_configuration.GetConnectionString("KeyVault"));
            return new SecretClient(vaultUri, new DefaultAzureCredential());
        }
    }
}
