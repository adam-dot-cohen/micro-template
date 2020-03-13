using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laso.Identity.Core.Extensions;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;

namespace Laso.Identity.Api
{
    public class LazyAzureKeyVaultConfigurationSource : IConfigurationSource
    {
        private readonly AzureKeyVaultConfigurationOptions _options;

        public LazyAzureKeyVaultConfigurationSource(AzureKeyVaultConfigurationOptions options)
        {
            _options = options;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new LazyAzureKeyVaultConfigurationProvider(_options.Client, _options.ReloadInterval);
        }
    }

    public static class LazyAzureKeyVaultConfigurationExtensions
    {
        public static IConfigurationBuilder AddLazyAzureKeyVault(this IConfigurationBuilder configurationBuilder, string vault, KeyVaultClient client, IKeyVaultSecretManager manager)
        {
            return configurationBuilder.Add(new LazyAzureKeyVaultConfigurationSource(new AzureKeyVaultConfigurationOptions
            {
                Client = client,
                Vault = vault,
                Manager = manager
            }));
        }
    }

    public class LazyAzureKeyVaultConfigurationProvider : ConfigurationProvider, IDisposable
    {
        private readonly TimeSpan? _reloadInterval;
        private readonly IKeyVaultClient _client;
        private readonly Dictionary<string, LoadedSecret> _loadedSecrets = new Dictionary<string, LoadedSecret>();

        private Task _pollingTask;
        private readonly CancellationTokenSource _cancellationToken;

        public LazyAzureKeyVaultConfigurationProvider(IKeyVaultClient client, TimeSpan? reloadInterval = null)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));

            if (reloadInterval != null && reloadInterval.Value <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(reloadInterval), reloadInterval, nameof(reloadInterval) + " must be positive.");
            }

            _pollingTask = null;
            _cancellationToken = new CancellationTokenSource();
            _reloadInterval = reloadInterval;
        }

        public override void Load()
        {
            LoadAsync().Wait();

            if (_pollingTask == null && _reloadInterval != null)
                _pollingTask = PollForSecretChangesAsync();
        }

        public override bool TryGet(string key, out string value)
        {
            var id = TranslateToKeyVault(key);

            lock (_loadedSecrets)
            {
                if (_loadedSecrets.ContainsKey(id))
                {
                    value = _loadedSecrets[id].Value;

                    return true;
                }

                var secret = _client.GetSecretAsync(id, _cancellationToken.Token).With(x => x.Wait()).Result;

                if (secret == null || secret.Attributes?.Enabled != true)
                {
                    value = null;

                    return false;
                }

                _loadedSecrets.Add(id, new LoadedSecret(key, secret.Value, secret.Attributes?.Updated));

                value = secret.Value;

                return true;
            }
        }

        private async Task PollForSecretChangesAsync()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(_reloadInterval.Value, _cancellationToken.Token);

                try
                {
                    await LoadAsync();
                }
                catch (Exception) { }
            }
        }

        private async Task LoadAsync()
        {
            IEnumerable<string> secretsKeys;

            lock (_loadedSecrets)
            {
                secretsKeys = _loadedSecrets.Keys;
            }

            var secrets = await Task.WhenAll(secretsKeys.Select(x => _client.GetSecretAsync(x, _cancellationToken.Token)));

            var hadChanges = false;

            lock (_loadedSecrets)
            {
                foreach (var secret in secrets)
                {
                    if (!_loadedSecrets[secret.Id].IsUpToDate(secret.Attributes?.Updated))
                    {
                        _loadedSecrets[secret.Id] = new LoadedSecret(TranslateToConfiguration(secret.Id), secret.Value, secret.Attributes?.Updated);

                        hadChanges = true;
                    }
                }

                if (hadChanges)
                {
                    var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var (_, value) in _loadedSecrets)
                        data.Add(value.Key, value.Value);

                    Data = data;
                }
            }

            if (hadChanges)
                OnReload();
        }

        public void Dispose()
        {
            _cancellationToken.Cancel();
        }

        private static string TranslateToConfiguration(string id)
        {
            return id.Replace("--", ConfigurationPath.KeyDelimiter);
        }

        private static string TranslateToKeyVault(string key)
        {
            return key.Replace(ConfigurationPath.KeyDelimiter, "--");
        }

        private readonly struct LoadedSecret
        {
            public LoadedSecret(string key, string value, DateTime? updated)
            {
                Key = key;
                Value = value;
                Updated = updated;
            }

            public string Key { get; }
            public string Value { get; }
            private DateTime? Updated { get; }

            public bool IsUpToDate(DateTime? updated)
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
