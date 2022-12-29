using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

using Library.Core;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Core.Services.Auth.Providers;

public class LoginGoogleProvider : IExternalLoginProvider
{
    private readonly string[] _clientIds;
    private readonly IMemoryCache _cache;

    public LoginGoogleProvider(IMemoryCache cache, string[] clientIds)
    {
        _cache = cache;
        _clientIds = clientIds;
    }

    public async Task<ExternalProviderUser> Login(string token)
    {
        try
        {
            // Validate jwt token

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwt = jwtHandler.ReadJwtToken(token) ?? throw new Exception("Failed to read token");

            // Source: https://ncona.com/2015/02/consuming-a-google-id-token-from-a-server/
            //
            using var http = new HttpClient();

            // Get Open ID configuration from Google
            if (!_cache.TryGetValue(nameof(OpenIdConfig), out string jwksUri))
            {
                var config = await http.GetAsync<OpenIdConfig>("https://accounts.google.com/.well-known/openid-configuration");
                if (config?.jwks_uri == null)
                {
                    throw new Exception("Failed to retrieve jwks uri from Google");
                }

                jwksUri = config.jwks_uri;
                _cache.Set(nameof(OpenIdConfig), jwksUri);
            }

            // Fetch certificate from Google
            if (!_cache.TryGetValue(nameof(JwksConfig), out JwksConfig jwksKey))
            {
                var jwk = await http.GetAsync<JwksKeys>(jwksUri);
                jwksKey = jwk?.keys?.FirstOrDefault(k => k.alg == jwt.Header.Alg && k.kid == jwt.Header.Kid);
                if (jwksKey == null)
                {
                    throw new Exception("Failed to find jwks keys");
                }
            }

            // Prepare signature verification
            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(new()
            {
                Modulus = FromBase64Url(jwksKey.n),
                Exponent = FromBase64Url(jwksKey.e)
            });

            // Validate jwt signature
            var validationParameters = new TokenValidationParameters
            {
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateLifetime = false,
                ValidAudiences = _clientIds,
                ValidIssuers = new string[] { "https://accounts.google.com", "accounts.google.com" },
                IssuerSigningKey = new RsaSecurityKey(rsa)
            };
            var principle = jwtHandler.ValidateToken(token, validationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken)
            {
                throw new Exception("Failed to verify jwt signature");
            }

            // Extract user claims from token
            var id = principle.GetClaim(ClaimTypes.NameIdentifier);
            var name = principle.GetClaim("name");
            var emailVerified = principle.GetClaim<bool>("email_verified");
            var email = principle.GetClaim(ClaimTypes.Email);

            // Validate data exists
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(email))
            {
                throw new Exception("Missing id or email");
            }

            // Ensure their email is verified
            if (!emailVerified)
            {
                throw new SiteException(ErrorCode.ExternalProviderUnverifiedEmail);
            }

            return new ExternalProviderUser(name, email, id);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw new SiteException(ErrorCode.ExternalProviderLoginFailed, "Google");
        }
    }

    private static byte[] FromBase64Url(string base64Url)
    {
        var padded = base64Url.Length % 4 == 0 ? base64Url : base64Url + "===="[(base64Url.Length % 4)..];
        var base64 = padded.Replace("_", "/").Replace("-", "+");

        return Convert.FromBase64String(base64);
    }

    private class JwksKeys
    {
        public IEnumerable<JwksConfig> keys { get; set; }
    }

    private class JwksConfig
    {
        public string alg { get; set; }

        public string kid { get; set; }

        public string n { get; set; }

        public string e { get; set; }
    }

    private class OpenIdConfig
    {
        public string jwks_uri { get; set; }
    }
}