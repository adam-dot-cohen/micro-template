namespace Laso.AdminPortal.Web.Configuration
{
    public class IdentityServiceOptions
    {
        public const string Section = "Services:Identity";
        public const string HttpClientName = "IdentityServiceClient";

        public string ServiceUrl { get; set; }
    }
}