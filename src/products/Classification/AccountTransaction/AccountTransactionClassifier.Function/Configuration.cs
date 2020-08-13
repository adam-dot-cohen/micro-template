using Microsoft.Extensions.Configuration;

namespace Insights.AccountTransactionClassifier.Function
{
    public static class Configuration
    {
        public static IConfiguration GetConfiguration(string workingDirectory)
        {
            return new ConfigurationBuilder()
                .SetBasePath(workingDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
