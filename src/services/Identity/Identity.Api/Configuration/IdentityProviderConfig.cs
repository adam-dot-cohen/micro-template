using System;
using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;

namespace Laso.Identity.Api.Configuration
{
    public static class IdentityProviderConfig
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
                new ApiResource("identity_api", "Identity Service API")
                {
                    UserClaims = new[] { IdentityServerConstants.StandardScopes.Email },
                    ApiSecrets = { new Secret("b39c84f6-3f3b-4d4e-8b43-84d4bd327257".Sha256()) }
                }
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
        public static IEnumerable<Client> GetClients(string clientUrl)
        {
            var redirectUriBuilder = new UriBuilder(clientUrl);
            string GetRedirectUrl(string path)
            {
                redirectUriBuilder.Path = path;
                return redirectUriBuilder.Uri.ToString();
            }

            return new List<Client>
            {
                // new Client
                // {
                    // ClientId = "monitoring_api",
                    // ClientSecrets = new [] { new Secret("b39c84f6-3f3b-4d4e-8b43-84d4bd327257".Sha256()) },
                    // AllowedGrantTypes = GrantTypes.ClientCredentials,
                    // AllowedScopes = new []
                    // {
                        // "identity_api"
                    // }
                // },
                new Client
                {
                    // IdentityTokenLifetime = 300, // defaults to 5 minutes, since only used initially
                    // AuthorizationCodeLifetime = 300, // 5 minute, since only used initially to get access token
                    // AccessTokenLifetime = 60 * 60, // defaults to 1 hour
                    // AbsoluteRefreshTokenLifetime = 60 * 60 * 24 * 30, // force login after 30 days
                    AccessTokenLifetime = 7 * 24 * 60 * 60, // Setting to 1 week until refresh token logic working.
                    ClientName = "Administration Portal",
                    ClientId = "adminportal_code",
                    ClientSecrets = new [] { new Secret("a3b5332e-68da-49a5-a5c0-99ded4b34fa3".Sha256()) },
                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials, // Authorization Code Flow with PKCE
                    RequirePkce = true,
                    AllowedScopes = new [] {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "identity_api"
                    },
                    // Allows use of access token when user is not authenticated, including refreshing tokens
                    AllowOfflineAccess = true,
                    UpdateAccessTokenClaimsOnRefresh = true,
                    AllowRememberConsent = false,
                    RequireConsent = false,
                    //AllowAccessTokensViaBrowser = true, // this is insecure
                    // Redirect to Open ID Connect middleware
                    RedirectUris = new [] { GetRedirectUrl("/signin-oidc") },
                    PostLogoutRedirectUris = { GetRedirectUrl("/signout-callback-oidc") }
                }
            };
        }
    }
}