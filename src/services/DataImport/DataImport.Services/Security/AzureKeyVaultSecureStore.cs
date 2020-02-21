using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Laso.DataImport.Core.Configuration;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Laso.DataImport.Services.Security
{
    public class AzureKeyVaultSecureStore : ISecureStore
    {
        private readonly IAzureKeyVaultConfiguration _config;

        public AzureKeyVaultSecureStore(IAzureKeyVaultConfiguration config)
        {
            _config = config;
        }

        public async Task<string> SetSecretAsync(string name, string value, CancellationToken cancellationToken)
        {
            var keyClient = GetKeyVaultClient();

            var url = _config.VaultBaseUrl;
            var result = await keyClient.SetSecretAsync(url, name, value, cancellationToken: cancellationToken).ConfigureAwait(false);

            return result.SecretIdentifier.Version;
        }

        public async Task<string> GetSecretAsync(string name, string version = null, CancellationToken cancellationToken = default)
        {
            var keyClient = GetKeyVaultClient();

            var url = _config.VaultBaseUrl;
            var result = await keyClient.GetSecretAsync(url, name, version ?? string.Empty, cancellationToken).ConfigureAwait(false);

            return result.Value;
        }

        public async Task<string> GetSecretOrDefaultAsync(string name, string version = null, string defaultValue = default, CancellationToken cancellationToken = default)
        {
            try
            {
                return await GetSecretAsync(name, version, cancellationToken).ConfigureAwait(false);
            }
            catch (KeyVaultErrorException e) when (e.Response.StatusCode == HttpStatusCode.NotFound)
            {
                return defaultValue;
            }
        }

        public async Task<(string Value, string Version)> GetCurrentSecretAsync(string name, CancellationToken cancellationToken)
        {
            var keyClient = GetKeyVaultClient();

            var url = _config.VaultBaseUrl;
            var bundle = await keyClient.GetSecretAsync(url, name, string.Empty, cancellationToken).ConfigureAwait(false);

            return (bundle.Value, bundle.SecretIdentifier.Version);
        }

        public async Task DeleteSecretAsync(string name, CancellationToken cancellationToken)
        {
            var keyClient = GetKeyVaultClient();

            var url = _config.VaultBaseUrl;
            await keyClient.DeleteSecretAsync(url, name, cancellationToken).ConfigureAwait(false);
        }

        private IKeyVaultClient GetKeyVaultClient()
        {
            var clientId = _config.ClientId;
            var applicationSecret = _config.Secret;

            var keyVaultClient = new KeyVaultClient(async (authority, resource, scope) =>
            {
                var clientCredential = new ClientCredential(clientId, applicationSecret);
                var authenticationContext = new AuthenticationContext(authority, TokenCache.DefaultShared);
                var accessTokenResult = await authenticationContext.AcquireTokenAsync(resource, clientCredential).ConfigureAwait(false);

                return accessTokenResult.AccessToken;
            });

            return keyVaultClient;
        }
    }
}
