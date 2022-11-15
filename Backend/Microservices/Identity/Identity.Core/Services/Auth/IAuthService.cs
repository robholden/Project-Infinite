
using Identity.Domain;

using Library.Core;

namespace Identity.Core.Services;

public interface IAuthService
{
    Task ForgetIdentity(Guid userId);

    Task<AuthToken> Login(string username, string password, ClientIdentity identity);

    Task<AuthToken> ExternalProviderLogin(string identifier, string email, ExternalProvider provider, ClientIdentity identity);

    Task<AuthToken> TouchIdLogin(Guid key, ClientIdentity identity);

    Task<AuthToken> RefreshAuthToken(Guid authTokenId, string refreshToken, bool enforce);

    Task VerifyAuth(Guid userId, VerifyAuthRequest request);

    Task Logout(Guid authTokenId);

    Task<AuthToken> ValidateTwoFactorCode(Guid userId, Guid authTokenId, string code, bool doNotAskAgain);

    Task<AuthToken> EnableDisableTouchId(Guid authTokenId, ClientIdentity identity, bool enabled);

    Task<AuthToken> Delete(Guid authTokenId);

    Task DeleteAll(Guid userId);
}