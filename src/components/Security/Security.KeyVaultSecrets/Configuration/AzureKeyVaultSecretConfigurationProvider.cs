using System;
using System.Collections.Concurrent;
using System.Net;
using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace Laso.Security.KeyVaultSecrets.Configuration
{
    public class AzureKeyVaultSecretConfigurationProvider : ConfigurationProvider
    {
        private readonly SecretClient _client;
        private readonly ConcurrentDictionary<string, LoadedSecret> _loadedSecrets
            = new ConcurrentDictionary<string, LoadedSecret>();

        public AzureKeyVaultSecretConfigurationProvider(SecretClient client, AzureKeyVaultSecretManager manager)
        {
            _client = client;
        }

        public override bool TryGet(string key, out string value)
        {
            // TODO: Check/expire cache lookup.

            var secretKey = key.Replace(ConfigurationPath.KeyDelimiter, "--");
            if (_loadedSecrets.TryGetValue(secretKey, out var existingSecret))
            {
                value = existingSecret.Value;
                return true;
            }
            
            KeyVaultSecret keyVaultSecret = null;
            try
            {
                var secret = _client.GetSecretAsync(secretKey).ConfigureAwait(false).GetAwaiter().GetResult();
                keyVaultSecret = secret.Value;
            }
            catch (RequestFailedException e)
            {
                // TODO: Check for key syntax and ignore things that could generate HTTP 400 error
                if ((e.Status != (int)HttpStatusCode.NotFound) 
                    && (e.Status != (int)HttpStatusCode.BadRequest))
                {
                    throw;
                }
            }

            if (keyVaultSecret == null)
            {
                value = null;
                return false;
            }

            var newSecret = new LoadedSecret(secretKey, keyVaultSecret.Value, keyVaultSecret.Properties.UpdatedOn);
            _loadedSecrets.TryAdd(key, newSecret);

            value = newSecret.Value;

            return true;
        }

        private class LoadedSecret
        {
            public LoadedSecret(string key, string value, DateTimeOffset? updated)
            {
                Key = key;
                Value = value;
                Updated = updated;
            }

            public string Key { get; }
            public string Value { get; }
            public DateTimeOffset? Updated { get; }

            public bool IsUpToDate(DateTimeOffset? updated)
            {
                if (updated.HasValue != Updated.HasValue)
                {
                    return false;
                }

                return updated.GetValueOrDefault() == Updated.GetValueOrDefault();
            }
        }
    }
}
