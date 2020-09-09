using System;
using Azure.Core;
using Azure.Identity;

namespace Insights.Data.Triggers.Services
{
    public interface IDatabricksWorkspaceAuthentication
    {
        string GetManagementToken();
        string GetDatabricksTokenAsync();
    }

    public class DatabricksWorkspaceAuthentication : IDatabricksWorkspaceAuthentication
    {
        private const string WellKnownDatabricksScopeId = "2ff814a6-3304-4ab8-85cb-cd0e6f879c1d";
        private const string AzureAdManagementScopeId = "https://management.core.windows.net/";

        private AccessToken? _managementToken;
        private AccessToken? _databricksToken;

        private readonly object _padLock = new object();

        public string GetManagementToken()
        {
            _managementToken = CreateTokenIfInvalid(_managementToken, AzureAdManagementScopeId);

            return _managementToken.Value.Token;
        }

        public string GetDatabricksTokenAsync()
        {
            _databricksToken = CreateTokenIfInvalid(_databricksToken, WellKnownDatabricksScopeId);

            return _databricksToken.Value.Token;
        }

        private AccessToken CreateTokenIfInvalid(AccessToken? token, string scope)
        {
            if (ShouldCreateToken(token))
            {
                lock (_padLock)
                {
                    if (ShouldCreateToken(token))
                    {
                        token = CreateToken(scope);
                    }
                }
            }

            return token.Value;
        }

        private bool ShouldCreateToken(AccessToken? token)
        {
            return token == null || DateTimeOffset.UtcNow >= token.Value.ExpiresOn.AddMinutes(-5);
        }

        private AccessToken CreateToken(string scope)
        {
            var credential = new DefaultAzureCredential();
            var requestContext = new TokenRequestContext(new[] { scope });
            return credential.GetToken(requestContext);
        }
    }
}