using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using LasoAuthenticationOptions = Laso.AdminPortal.Infrastructure.Configuration.AuthenticationOptions;

namespace Laso.AdminPortal.Infrastructure
{
    public class BearerTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<BearerTokenHandler> _logger;
        private readonly LasoAuthenticationOptions _authenticationOptions;

        public BearerTokenHandler(
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IOptionsMonitor<LasoAuthenticationOptions> authenticationOptions,
            ILogger<BearerTokenHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _authenticationOptions = authenticationOptions.CurrentValue;
        }
    
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                // Handle user scenario
                var accessToken = await GetUserAccessTokenAsync();

                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    request.SetBearerToken(accessToken);
                }

                return await base.SendAsync(request, cancellationToken);
            }

            // Handle hosted services scenario
            await SetClientCredentialsBearerToken(request, cancellationToken);

            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                _clientCredentialsAccessToken = null;
                _clientCredentialsExpiration = null;
            }

            return response;
        }

        private static string _clientCredentialsAccessToken;
        private static DateTimeOffset? _clientCredentialsExpiration;

        private async Task SetClientCredentialsBearerToken(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_clientCredentialsAccessToken != null && _clientCredentialsExpiration.HasValue)
            {
                if (DateTimeOffset.UtcNow.AddSeconds(-60) < _clientCredentialsExpiration.Value)
                {
                    // Not yet expired, use existing token
                    request.SetBearerToken(_clientCredentialsAccessToken);
                    return;
                }

                _clientCredentialsAccessToken = null;
                _clientCredentialsExpiration = null;
            }

            // Get a new token
            var idpClient = _httpClientFactory.CreateClient("IDPClient");
            var discoveryResponse = await idpClient.GetDiscoveryDocumentAsync(cancellationToken: cancellationToken);
            if (discoveryResponse.IsError)
            {
                return;
            }

            var tokenResponse = await idpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = discoveryResponse.TokenEndpoint,
                ClientId = _authenticationOptions.ClientId,
                ClientSecret = _authenticationOptions.ClientSecret,
                Scope = "identity_api"
            }, cancellationToken);

            if (!tokenResponse.IsError)
            {
                _clientCredentialsAccessToken = tokenResponse.AccessToken;
                _clientCredentialsExpiration = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(tokenResponse.ExpiresIn);
                request.SetBearerToken(tokenResponse.AccessToken);
            }
        }

        // refresh token for user scenario
        private async Task<string> GetUserAccessTokenAsync()
        {
            // get the expires_at value & parse it
            var expiresAt = await _httpContextAccessor.HttpContext.GetTokenAsync("expires_at");
    
            var expiresAtAsDateTimeOffset = DateTimeOffset.Parse(expiresAt, CultureInfo.InvariantCulture);
    
            if ((expiresAtAsDateTimeOffset.AddSeconds(-60)).ToUniversalTime() > DateTime.UtcNow)
            {
                // no need to refresh, return the access token
                return await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            }
    
            _logger.LogInformation("Refreshing access token");
            var idpClient = _httpClientFactory.CreateClient("IDPClient");
    
            // get the discovery document
            var discoveryResponse = await idpClient.GetDiscoveryDocumentAsync();
    
            // refresh the tokens
            var refreshToken = await _httpContextAccessor
                       .HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);
    
            var refreshResponse = await idpClient.RequestRefreshTokenAsync(
                new RefreshTokenRequest
                {
                    Address = discoveryResponse.TokenEndpoint,
                    ClientId = _authenticationOptions.ClientId,
                    ClientSecret = _authenticationOptions.ClientSecret,
                    RefreshToken = refreshToken
                });

            if (refreshResponse.IsError)
            {
                _logger.LogWarning("Failed to refresh token {@TokenError}",
                    new
                    {
                        refreshResponse.ErrorType,
                        refreshResponse.ErrorDescription,
                        refreshResponse.Error,
                        refreshResponse.HttpErrorReason,
                        refreshResponse.Exception
                    });
                return null;
            }
    
            // store the tokens             
            var updatedTokens = new List<AuthenticationToken>();
            updatedTokens.Add(new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.IdToken,
                Value = refreshResponse.IdentityToken
            });
            updatedTokens.Add(new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.AccessToken,
                Value = refreshResponse.AccessToken
            });
            updatedTokens.Add(new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.RefreshToken,
                Value = refreshResponse.RefreshToken
            });
            updatedTokens.Add(new AuthenticationToken
            {
                Name = "expires_at",
                Value = (DateTime.UtcNow + TimeSpan.FromSeconds(refreshResponse.ExpiresIn)).
                        ToString("o", CultureInfo.InvariantCulture)
            });
    
            // get authenticate result, containing the current principal & 
            // properties
            var currentAuthenticateResult = await _httpContextAccessor
                .HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    
            // store the updated tokens
            currentAuthenticateResult.Properties.StoreTokens(updatedTokens);
    
            // sign in
            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                currentAuthenticateResult.Principal,
                currentAuthenticateResult.Properties);

            _logger.LogInformation("Refreshed access token");

            return refreshResponse.AccessToken;
        }
    }
}