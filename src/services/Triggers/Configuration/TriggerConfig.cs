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

        public string DataBricksBaseUri => GetSetting<string>("databricks_baseuri");
        public string DataBricksBearerToken => GetSetting<string>("bearerToken");

        private T GetSetting<T>(string key)
        {
            return _configuration.GetValue<T>(key);
        }
    }

    public interface ITriggerConfig
    {
        public string DataBricksBaseUri { get; }
        public string DataBricksBearerToken { get; }
    }
}