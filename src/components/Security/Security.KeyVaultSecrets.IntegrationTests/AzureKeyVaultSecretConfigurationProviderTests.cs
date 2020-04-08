using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Laso.Security.KeyVaultSecrets.Configuration;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

namespace Security.KeyVaultSecrets.IntegrationTests
{
    public class AzureKeyVaultSecretConfigurationProviderTests
    {
        [Fact]
        public void When_Key_Exists_Should_Succeed()
        {
            var _configuration = new ConfigurationBuilder()
                .AddJsonFile("test.appsettings.json")
                .Build();

            var serviceUri = new Uri(_configuration["AzureKeyVault:ServiceUrl"]);

            var secretClient = new SecretClient(serviceUri, new DefaultAzureCredential());
            var configurationProvider = new AzureKeyVaultSecretConfigurationProvider(secretClient, new AzureKeyVaultSecretManager());
            var result = configurationProvider.TryGet(
                "0a875863-01f7-4ea0-9015-39bb5ef35568-laso-pgp-passphrase",
                out var secret);

            result.ShouldBeTrue();
            secret.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public void When_Key_Doesnt_Exist_Should_Succeed()
        {
            var _configuration = new ConfigurationBuilder()
                .AddJsonFile("test.appsettings.json")
                .Build();

            var serviceUri = new Uri(_configuration["AzureKeyVault:ServiceUrl"]);

            var secretClient = new SecretClient(serviceUri, new DefaultAzureCredential());
            var configurationProvider = new AzureKeyVaultSecretConfigurationProvider(secretClient, new AzureKeyVaultSecretManager());
            var result = configurationProvider.TryGet("xyz-laso-pgp-passphrase", out var secret);

            result.ShouldBeFalse();
            secret.ShouldBeNull();
        }
    }
}
