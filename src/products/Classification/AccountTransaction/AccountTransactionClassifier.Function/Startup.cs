using System;
using Insights.AccountTransactionClassifier.Function.Azure;
using Insights.AccountTransactionClassifier.Function.Classifier;
using Insights.AccountTransactionClassifier.Function.Normalizer;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Insights.AccountTransactionClassifier.Function.Startup))]

namespace Insights.AccountTransactionClassifier.Function
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddConfiguration()
                .AddAzureBankAccountTransactionClassifier();
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services)
        {
            var localRoot = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
            var azureRoot = $"{Environment.GetEnvironmentVariable("HOME")}/site/wwwroot";
            var workingDirectory = localRoot ?? azureRoot;

            var configuration = new ConfigurationBuilder()
                .SetBasePath(workingDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton(configuration);

            return services;
        }

        public static IServiceCollection AddAzureBankAccountTransactionClassifier(this IServiceCollection services)
        {
            services.AddTransient<IAccountTransactionNormalizer, AccountTransactionNormalizer>();

            services.AddTransient<IRetryPolicy, RetryPolicy>();
            services.AddTransient<IMachineLearningService, AzureMachineLearningService>();

            services.AddTransient<IAccountTransactionClassifier>(sp =>
            {
                var configuration = sp.GetService<IConfiguration>();

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
