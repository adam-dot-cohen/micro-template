using System.Collections.Generic;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;

namespace Laso.Identity.Api.Configuration
{
    public class Config
    {
        // ApiResources define the apis in your system
        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
            {
                new ApiResource("provisioning", "Provisioning Service")
                {
                    // Specify which user claims may be passed to API Resources
                    // These claims will be encoded into the access token (in addition to the id_token)
                    UserClaims = new[]{ IdentityServerConstants.StandardScopes.Email }
                },
                new ApiResource("identity", "Identity Service"),
            };
        }

        // Identity resources are data like user ID, name, or email address of a user
        // see: http://docs.identityserver.io/en/release/configuration/resources.html
        public static IEnumerable<IdentityResource> GetResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email()
            };
        }

        // client want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients(Dictionary<string, string> clientsUrl = null)
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "test",
                    ClientSecrets = new [] { new Secret("secret".ToSha256()) },
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    AllowedScopes = new []
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile
                    }
                },
                new Client
                {
                    ClientId = "laso_code",
                    ClientSecrets = new [] { new Secret("secret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.Hybrid, // Authorization Code Flow with OpenID Connect
                    AllowedScopes = new [] {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "provisioning"
                    },
                    // Allows use of access token when user is not authenticated
                    AllowOfflineAccess = true,
                    AllowAccessTokensViaBrowser = true, // this is insecure
                    // Redirect to Open ID Connect middleware
                    RedirectUris = new [] { "https://localhost:5001/signin-oidc" },
                    PostLogoutRedirectUris = { "https://localhost:5001/signout-callback-oidc" },
                }
            };
        }
    }
}