using System;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Laso.Hosting.Extensions
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddAzureKeyVault(this IConfigurationBuilder builder, string serviceUrl, HostBuilderContext context)
        {
            // TODO: Eventually remove the "IsDevelopment" condition after we trim down the application secrets vault.
            if (!context.HostingEnvironment.IsDevelopment() && !context.IsTest())
                builder.AddAzureKeyVault(new Uri(serviceUrl), new DefaultAzureCredential());

            return builder;
        }
    }
}
