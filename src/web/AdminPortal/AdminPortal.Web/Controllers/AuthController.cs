using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Laso.AdminPortal.Web.Controllers
{
    [Authorize]
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var identityToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken);
            var authInfo = new
            {
                IdentityToken = identityToken,
                Claims = User.Claims.Select(c => new {c.Type, c.Value})
            };
            _logger.LogInformation("Successfully logged in {@authInfo}", authInfo);

            // Redirect to main application
            return Redirect("/");        }

        public async Task Logout()
        {
            // TODO: Revoke tokens first

            // Clears the  local cookie ("Cookies" must match name from scheme)
            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync("oidc");

            _logger.LogInformation("Successfully logged out");

            // return Redirect("/");
        }
    }
}