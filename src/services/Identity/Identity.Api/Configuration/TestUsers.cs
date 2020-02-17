using System.Collections.Generic;
using System.Security.Claims;
using IdentityModel;
using IdentityServer4.Test;

namespace Laso.Identity.Api.Configuration
{
    public static class TestUsers
    {
        public static List<TestUser> Users() => new List<TestUser>
        {
            new TestUser
            {
                SubjectId = "818727", Username = "ollie", Password = "ollie",
                Claims =
                {
                    new Claim(JwtClaimTypes.Name, "Ollie Persona"),
                    new Claim(JwtClaimTypes.GivenName, "Ollie"),
                    new Claim(JwtClaimTypes.FamilyName, "Persona"),
                    new Claim(JwtClaimTypes.Email, "ollie.persona@mailinator.com"),
                    new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean)
                }
            },
            new TestUser
            {
                SubjectId = "88421113", Username = "andy", Password = "andy",
                Claims =
                {
                    new Claim(JwtClaimTypes.Name, "Andy Persona"),
                    new Claim(JwtClaimTypes.GivenName, "Andy"),
                    new Claim(JwtClaimTypes.FamilyName, "Persona"),
                    new Claim(JwtClaimTypes.Email, "andy.persona@mailinator.com"),
                    new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                }
            }
        };
    }
}