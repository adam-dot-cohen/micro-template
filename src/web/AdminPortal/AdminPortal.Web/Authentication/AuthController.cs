using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Laso.AdminPortal.Web.Authentication
{
    [Authorize]
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("")]
        [Route("login")]
        public async Task<IActionResult> Login([FromQuery] string returnUrl = null)
        {
            var identityToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken);
            var authInfo = new
            {
                IdentityToken = identityToken,
                Claims = User.Claims.Select(c => new {c.Type, c.Value})
            };
            _logger.LogInformation("Successfully logged in {@authInfo}", authInfo);

            // Redirect to main application
            return Redirect(returnUrl ?? "/");
        }

        [HttpGet]
        [Route("logout")]
        public async Task Logout()
        {
            // TODO: Revoke tokens first

            // Clears the  local cookie ("Cookies" must match name from scheme)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

            _logger.LogInformation("Successfully logged out");
        }
    }
}