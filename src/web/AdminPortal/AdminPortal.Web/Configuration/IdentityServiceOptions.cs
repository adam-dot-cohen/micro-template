namespace Laso.AdminPortal.Web.Configuration
{
    public class ServicesOptions
    {
        public const string Section = "Services";

        public IdentityServiceOptions Identity { get; set; }
    }

    public class IdentityServiceOptions
    {
        public const string Section = "Services:Identity";

        public string AuthorityUrl { get; set; }
        public string ServiceUrl { get; set; }
    }
}