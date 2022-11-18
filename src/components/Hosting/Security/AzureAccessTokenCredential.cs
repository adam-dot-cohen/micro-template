using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace Laso.Hosting.Security
{
    public class AzureAccessTokenCredential : TokenCredential
    {
        private readonly IConfiguration _configuration;

        public AzureAccessTokenCredential(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            if (requestContext.Scopes.Length != 1)
                throw new InvalidOperationException("Unexpected token scopes.");

            var scopeName = new Uri(requestContext.Scopes[0]).Host.Replace('.', '-');
            var tokenKey = $"AccessToken:{scopeName}";
            var encodedToken = _configuration[tokenKey];
            if (encodedToken == null)
                throw new CredentialUnavailableException("Access Token not found.");

            var token = Decode(encodedToken);
            var result = new AccessToken(token.AccessToken, token.ExpiresOn);

            return new ValueTask<AccessToken>(result);
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private static AzureAccessToken Decode(string encodedToken)
        {
            var decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(encodedToken));
            var token = JsonSerializer.Deserialize<AzureAccessToken>(
                decodedToken,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new DateTimeOffsetConverter() }
                });

            return token!;
        }

        private class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
        {
            public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return DateTimeOffset.Parse(reader.GetString()!);
            }

            public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }

        private class AzureAccessToken
        {
            public string TokenType { get; set; } = null!;
            public string AccessToken { get; set; } = null!;
            public DateTimeOffset ExpiresOn { get; set; }
            public Guid Subscription { get; set; }
            public Guid Tenant { get; set; }
        }
    }
}