namespace Laso.AdminPortal.Web.Configuration
{
    public class AuthenticationOptions
    {
        public const string Section = "Authentication";

        public string AuthorityUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}