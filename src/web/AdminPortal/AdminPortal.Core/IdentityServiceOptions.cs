namespace Laso.AdminPortal.Core
{
    public class IdentityServiceOptions
    {
        public const string Section = "Services:Identity";

        public string ServiceUrl { get; set; }
    }

    public class ProvisioningServiceOptions
    {

        public const string Section = "Services:Provisioning";

        public string ServiceUrl { get; set; }
    }
}