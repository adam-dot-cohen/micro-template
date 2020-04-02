﻿using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Hosting;

namespace Laso.Provisioning.Api.Configuration
{
    public static class AzureKeyVaultExtensions
    {
        public static IConfigurationBuilder AddAzureKeyVault(this IConfigurationBuilder builder, IConfiguration configuration, HostBuilderContext context)
        {
            var vaultUri = configuration["Services:Provisioning:Configuration:ServiceUrl"];

            // If we have an access token configured for development environment, then
            // attempt to use it. This is useful for the case where we are attempting
            // to debug locally in a Docker container. (See scripts\get-access-token.ps1
            // for storing access tokens locally in the local User Secret store (which
            // is accessible from the container.)
            var accessToken = configuration["AzureKeyVault:AccessToken"];
            if (context.HostingEnvironment.IsDevelopment()
                && !string.IsNullOrEmpty(accessToken))
            {
                var keyVaultClient = new KeyVaultClient(
                    (a, r, s) => Task.FromResult(accessToken));

                builder.AddAzureKeyVault(vaultUri, keyVaultClient, new DefaultKeyVaultSecretManager());
            }
            else
            {
                builder.AddAzureKeyVault(vaultUri);
            }

            return builder;
        }
    }
}
