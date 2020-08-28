﻿using System;
using System.IO;
using Azure.Identity;
using Insights.AccountTransactionClassifier.Function.Azure;
using Insights.AccountTransactionClassifier.Function.Classifier;
using Insights.AccountTransactionClassifier.Function.Normalizer;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Insights.AccountTransactionClassifier.Function.Startup))]

namespace Insights.AccountTransactionClassifier.Function
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = GetConfiguration();
            Configure(builder, configuration);
        }

        public void Configure(IFunctionsHostBuilder builder, IConfigurationRoot hostConfiguration)
        {
            var keyVaultServiceUrl = hostConfiguration["Products:AccountTxnClassifier:ConfigSecrets:ServiceUrl"];

            // Add KeyVaultSecrets to Configuration
            var configuration = new ConfigurationBuilder()
                .AddConfiguration(hostConfiguration)
                .AddAzureKeyVault(new Uri(keyVaultServiceUrl), new DefaultAzureCredential())
                .Build();

            builder.Services
                .AddSingleton(configuration)
                .AddAzureBankAccountTransactionClassifier()
                .AddAzureClients(factoryBuilder => factoryBuilder.AddBlobServiceClient(
                    new Uri(configuration["Services:Provisioning:PartnerEscrowStorage:ServiceUrl"])));
        }

        private static IConfigurationRoot GetConfiguration()
        {
            var localRoot = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
            
            var azureRoot = $"{Environment.GetEnvironmentVariable("HOME")}/site/wwwroot";
            if (!Directory.Exists(azureRoot))
                azureRoot = null;

            var workingDirectory = localRoot ?? azureRoot ?? Directory.GetCurrentDirectory();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(workingDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            return configuration;
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureBankAccountTransactionClassifier(this IServiceCollection services)
        {
            services.AddTransient<IAccountTransactionNormalizer, AccountTransactionNormalizer>();

            services.AddTransient<IRetryPolicy, RetryPolicy>();
            services.AddTransient<IMachineLearningService, AzureMachineLearningService>();

            services.AddTransient<IAccountTransactionClassifier>(sp =>
            {
                var configuration = sp.GetService<IConfiguration>();

                // TODO: Eventually, these should be part of product configuration. [jay_mclain]
                var creditsMachineLearningService = sp.GetService<IMachineLearningService>();
                creditsMachineLearningService.BaseUrl = configuration["Components:AzureCreditsBankTransactionClassifier:Endpoint"];
                creditsMachineLearningService.ApiKey = configuration["Components:AzureCreditsBankTransactionClassifier:Key"];

                var debitsMachineLearningService = sp.GetService<IMachineLearningService>();
                debitsMachineLearningService.BaseUrl = configuration["Components:AzureDebitsBankTransactionClassifier:Endpoint"];
                debitsMachineLearningService.ApiKey = configuration["Components:AzureDebitsBankTransactionClassifier:Key"];

                var normalizer = sp.GetRequiredService<IAccountTransactionNormalizer>();

                return new AzureBankAccountTransactionClassifier(
                    normalizer, creditsMachineLearningService, debitsMachineLearningService);
            });

            return services;
        }
    }
}
