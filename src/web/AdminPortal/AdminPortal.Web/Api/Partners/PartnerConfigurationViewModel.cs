namespace Laso.AdminPortal.Web.Api.Partners
{
    public class PartnerConfigurationViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public ConfigurationSetting[] Settings { get; set; }
    }

    public class ConfigurationSetting
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
