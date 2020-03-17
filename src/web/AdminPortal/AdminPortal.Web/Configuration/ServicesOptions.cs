using Laso.AdminPortal.Core;

namespace Laso.AdminPortal.Web.Configuration
{
    public class ServicesOptions
    {
        public const string Section = "Services";

        public IdentityServiceOptions Identity { get; set; }
    }
}