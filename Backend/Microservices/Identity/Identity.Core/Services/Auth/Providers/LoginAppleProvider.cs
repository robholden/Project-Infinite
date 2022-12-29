using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

using Library.Core;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Core.Services.Auth.Providers;

public class LoginAppleProvider : IExternalLoginProvider
{
    private readonly string[] _clientIds;
    private readonly IMemoryCache _cache;

    public LoginAppleProvider(IMemoryCache cache, string[] clientIds)
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

            // Source: https://developer.apple.com/documentation/sign_in_with_apple/sign_in_with_apple_rest_api/authenticating_users_with_sign_in_with_apple
            //
            using var http = new HttpClient();

            // Fetch certificate from Apple
            if (!_cache.TryGetValue(nameof(JwksConfig), out JwksConfig jwksKey))
            {
                var jwk = await http.GetAsync<JwksKeys>("https://appleid.apple.com/auth/keys");
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
                ValidIssuer = "https://appleid.apple.com",
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
            throw new SiteException(ErrorCode.ExternalProviderLoginFailed, "Apple");
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
}