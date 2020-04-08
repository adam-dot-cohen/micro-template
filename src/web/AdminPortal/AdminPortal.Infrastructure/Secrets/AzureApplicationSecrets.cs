using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Laso.AdminPortal.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using KeyVaultSecret = Laso.AdminPortal.Core.KeyVaultSecret;

namespace Laso.AdminPortal.Infrastructure.Secrets
{
    public class AzureApplicationSecrets : IApplicationSecrets
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureApplicationSecrets> _logger;

        public AzureApplicationSecrets(IConfiguration configuration, ILogger<AzureApplicationSecrets> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<KeyVaultSecret> GetSecret(string name, CancellationToken cancellationToken)
        {
            var keyClient = GetSecretClient();

            KeyVaultSecret secret = null;

            try
            {
                var response = await keyClient.GetSecretAsync(name, cancellationToken: cancellationToken);

                secret = new KeyVaultSecret
                {
                    Value = response.Value.Value,
                    Version = response.Value.Properties.Version,
                    Reference = response.Value.Properties.VaultUri
                };
            }
            catch (RequestFailedException e)
            {
                if (e.Status != 404)
                    _logger.LogCritical(e, e.Message);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Unexpected failure retrieving secret.");
            }

            return secret;
        }
        
        private SecretClient GetSecretClient()
        {
            var vaultBaseUri = new Uri(_configuration["Services:Provisioning:Partner.Secrets:ServiceUrl"]);
            var credential = new DefaultAzureCredential();
            var secretClient = new SecretClient(vaultBaseUri, credential);

            return secretClient;
        }
    }
}
