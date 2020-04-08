using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace Laso.Security.KeyVaultSecrets.Configuration
{
    public static class AzureKeyVaultSecretConfigurationExtensions
    {
        //public static IConfigurationBuilder AddAzureKeyVaultSecrets(this IConfigurationBuilder builder, IConfiguration configuration, HostBuilderContext context)
        //{
        //    var vaultUrl = configuration["Services:Provisioning:Configuration:ServiceUrl"];

        //    // If we have an access token configured for development environment, then
        //    // attempt to use it. This is useful for the case where we are attempting
        //    // to debug locally in a Docker container. (See scripts\get-access-token.ps1
        //    // for storing access tokens locally in the local User Secret store (which
        //    // is accessible from the container.)
        //    var accessToken = configuration["AzureKeyVault:AccessToken"];
        //    if (context.HostingEnvironment.IsDevelopment()
        //        && !string.IsNullOrEmpty(accessToken))
        //    {
        //        var keyVaultClient = new KeyVaultClient((a, r, s) => Task.FromResult(accessToken));
        //        //builder.AddAzureAzureKeyVaultSecrets(vaultUrl, keyVaultClient, new DefaultKeyVaultSecretManager());
        //    }
        //    else if (!context.HostingEnvironment.IsDevelopment())
        //    {
        //        builder.AddAzureAzureKeyVaultSecrets(vaultUrl);
        //    }

        //    return builder;
        //}

        public static IConfigurationBuilder AddAzureAzureKeyVaultSecrets(this IConfigurationBuilder configurationBuilder, string vaultUrl)
        {
            configurationBuilder.Add(new AzureKeyVaultSecretConfigurationSource(
                new AzureKeyVaultSecretConfigurationOptions
                {
                    Client = new SecretClient(new Uri(vaultUrl), new DefaultAzureCredential()),
                    Manager = AzureKeyVaultSecretManager.Instance
                }));

            return configurationBuilder;
        }
    }
}
