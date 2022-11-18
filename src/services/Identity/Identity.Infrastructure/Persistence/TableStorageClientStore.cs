using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Laso.Identity.Infrastructure.Extensions;
using Laso.TableStorage;
using IdentityServerClient = IdentityServer4.Models.Client;
using IdentityServerSecret = IdentityServer4.Models.Secret;
using SystemClaim = System.Security.Claims.Claim;

namespace Laso.Identity.Infrastructure.Persistence
{
    public class TableStorageClientStore : IClientStore
    {
        private readonly ITableStorageService _tableStorageService;

        public TableStorageClientStore(ITableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        public async Task<IdentityServerClient> FindClientByIdAsync(string clientId)
        {
            var client = _tableStorageService.GetAsync<Client>(clientId);
            var claims = _tableStorageService.GetAllAsync<Claim>(x=> x.ClientId == clientId);
            var secrets = _tableStorageService.GetAllAsync<ClientSecret>(x=> x.ClientId == clientId);

            await Task.WhenAll(client, claims, secrets);

            return client.Result.To(x => new IdentityServerClient
            {
                Enabled = x.Enabled,
                ClientId = x.ClientId,
                ProtocolType = x.ProtocolType,
                RequireClientSecret = x.RequireClientSecret,
                ClientName = x.ClientName,
                Description = x.Description,
                ClientUri = x.ClientUri,
                LogoUri = x.LogoUri,
                RequireConsent = x.RequireConsent,
                AllowRememberConsent = x.AllowRememberConsent,
                AllowedGrantTypes = x.AllowedGrantTypes,
                RequirePkce = x.RequirePkce,
                AllowPlainTextPkce = x.AllowPlainTextPkce,
                AllowAccessTokensViaBrowser = x.AllowAccessTokensViaBrowser,
                RedirectUris = x.RedirectUris,
                PostLogoutRedirectUris = x.PostLogoutRedirectUris,
                FrontChannelLogoutUri = x.FrontChannelLogoutUri,
                FrontChannelLogoutSessionRequired = x.FrontChannelLogoutSessionRequired,
                BackChannelLogoutUri = x.BackChannelLogoutUri,
                BackChannelLogoutSessionRequired = x.BackChannelLogoutSessionRequired,
                AllowOfflineAccess = x.AllowOfflineAccess,
                AllowedScopes = x.AllowedScopes,
                AlwaysIncludeUserClaimsInIdToken = x.AlwaysIncludeUserClaimsInIdToken,
                IdentityTokenLifetime = x.IdentityTokenLifetime,
                AccessTokenLifetime = x.AccessTokenLifetime,
                AuthorizationCodeLifetime = x.AuthorizationCodeLifetime,
                AbsoluteRefreshTokenLifetime = x.AbsoluteRefreshTokenLifetime,
                SlidingRefreshTokenLifetime = x.SlidingRefreshTokenLifetime,
                ConsentLifetime = x.ConsentLifetime,
                RefreshTokenUsage = x.RefreshTokenUsage,
                UpdateAccessTokenClaimsOnRefresh = x.UpdateAccessTokenClaimsOnRefresh,
                RefreshTokenExpiration = x.RefreshTokenExpiration,
                AccessTokenType = x.AccessTokenType,
                EnableLocalLogin = x.EnableLocalLogin,
                IdentityProviderRestrictions = x.IdentityProviderRestrictions,
                IncludeJwtId = x.IncludeJwtId,
                AlwaysSendClientClaims = x.AlwaysSendClientClaims,
                ClientClaimsPrefix = x.ClientClaimsPrefix,
                PairWiseSubjectSalt = x.PairWiseSubjectSalt,
                UserSsoLifetime = x.UserSsoLifetime,
                UserCodeType = x.UserCodeType,
                DeviceCodeLifetime = x.DeviceCodeLifetime,
                AllowedCorsOrigins = x.AllowedCorsOrigins,
                Properties = x.Properties,
                Claims = claims.Result
                    .Select(y => new ClientClaim(y.Type, y.Value))
                    .ToList(),
                ClientSecrets = secrets.Result
                    .Select(y => new IdentityServerSecret
                    {
                        Description = y.Description,
                        Value = y.Value,
                        Expiration = y.Expiration,
                        Type = y.Type
                    })
                    .ToList()
            });
        }
    }
}
