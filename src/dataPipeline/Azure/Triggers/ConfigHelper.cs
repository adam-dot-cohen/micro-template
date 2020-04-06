using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;

namespace Insights.Data.Triggers
{
    internal class ConfigHelper
    {
        private IConfigurationRoot _config;
        public ConfigHelper(ExecutionContext context)
        {
            _config = new ConfigurationBuilder()
                             .SetBasePath(context.FunctionAppDirectory)
                             .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                             .AddEnvironmentVariables()
                             .Build();
        }

        public IConfiguration Config => _config;

    }
}
