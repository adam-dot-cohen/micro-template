using Microsoft.Extensions.Configuration;

namespace Insights.Data.Triggers.Configuration
{
    public class TriggerConfig : ITriggerConfig
    {
        private readonly IConfiguration _configuration;

        public TriggerConfig(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string DatabricksBaseUri => GetSetting<string>("databricks_baseuri");
        public string DatabricksWorkspaceResourceId => GetSetting<string>("databricks_resource_id");

        private T GetSetting<T>(string key)
        {
            return _configuration.GetValue<T>(key);
        }
    }

    public interface ITriggerConfig
    {
        string DatabricksBaseUri { get; }
        string DatabricksWorkspaceResourceId { get; }
    }
}