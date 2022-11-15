
using Library.Core;

namespace Identity.Core.Services.Auth.Providers;

public class LoginFacebookProvider : IExternalLoginProvider
{
    private readonly string _secret;

    public LoginFacebookProvider(string secret)
    {
        _secret = secret;
    }

    public async Task<ExternalProviderUser> Login(string token)
    {
        try
        {
            using var http = new HttpClient();

            // Call facebook api to verify api token
            var proof = token.HashHmacSha256(_secret);
            var url = $"https://graph.facebook.com/me?access_token={token}&fields=name,email&appsecret_proof={proof}";
            var facebookUser = await http.GetAsync<FacebookUser>(url);

            // Validate data exists
            if (string.IsNullOrEmpty(facebookUser?.id) || string.IsNullOrEmpty(facebookUser?.email))
            {
                throw new Exception("Missing id or email");
            }

            return new ExternalProviderUser(facebookUser.name, facebookUser.email, facebookUser.id);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw new SiteException(ErrorCode.ExternalProviderLoginFailed, "Facebook");
        }
    }

    private class FacebookUser
    {
        public string id { get; set; }

        public string name { get; set; }

        public string email { get; set; }
    }
}