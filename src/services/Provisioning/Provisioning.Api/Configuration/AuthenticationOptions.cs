namespace Laso.Provisioning.Api.Configuration
{
    public class AuthenticationOptions
    {
        public const string Section = "Authentication";

        public bool Enabled { get; set; }
        public string AuthorityUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}