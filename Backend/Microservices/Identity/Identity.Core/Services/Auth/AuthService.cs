using System.Security.Cryptography;

using Identity.Domain;

using Library.Core;

using Microsoft.EntityFrameworkCore;

namespace Identity.Core.Services;

public class AuthService : IAuthService
{
    private readonly IdentityContext _ctx;
    private readonly IPasswordService _passwordService;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IUserService _userService;

    public AuthService(IdentityContext ctx, IUserService userService, IPasswordService passwordService, ITwoFactorService twoFactorService)
    {
        _ctx = ctx;
        _userService = userService;
        _passwordService = passwordService;
        _twoFactorService = twoFactorService;
    }

    public async Task<AuthToken> Login(string username, string password, ClientIdentity identity)
    {
        // Find user
        username = username.ToLower().Trim();
        var user = await _ctx.Users
            .FindAsync(x => x.ExternalProvider == ExternalProvider.Unset && (x.Username.ToLower() == username || x.Email.ToLower() == username), ErrorCode.IncorrectUsernameOrPassword);

        // Verify password is correct
        var loggedIn = await _passwordService.Check(user.UserId, password);

        // Go to user service to handle failed login attempts
        var status = await _userService.LoginAttempted(user, loggedIn);
        EnsureAccountIsEnabled(status);

        if (!loggedIn)
        {
            throw new SiteException(ErrorCode.IncorrectUsernameOrPassword);
        }

        // Do we need two factor verification?
        var skip2fa = !user.TwoFactorEnabled;
        if (!skip2fa)
        {
            skip2fa = await _ctx.AuthTokens.AnyAsync(t =>
                t.IdentityKey == identity.Key &&
                t.PlatformRaw == identity.Platform &&
                t.TwoFactorPassed &&
                t.RememberIdentityForTwoFactor
            );
        }

        // Create token model
        var token = new AuthToken
        {
            UserId = user.UserId,
            IdentityKey = identity.Key,
            IpAddress = identity.IpAddress,
            PlatformRaw = identity.Platform,
            TwoFactorPassed = skip2fa,
            RefreshToken = GenerateRefreshToken()
        };
        token.SetPlatform();
        token = await _ctx.CreateAsync(token);

        // If 2FA is enabled send to user
        // Only trigger when identity is not set as remembered
        if (!skip2fa)
        {
            await _userService.Trigger2FA(user.UserId);
        }

        // Mark user as active
        await _userService.UpdateLastActive(user.UserId);

        // Return reponse
        return token;
    }

    public async Task<AuthToken> ExternalProviderLogin(string identifier, string email, ExternalProvider provider, ClientIdentity identity)
    {
        // Find user via identity
        var user = await _ctx.Users.FindOrNullAsync(x => x.ExternalProviderIdentifier == identifier);

        // Check user exists
        if (user == null)
        {
            // Find user via email
            user = await _ctx.Users.FindOrNullAsync(x => x.Email == email.ToLower());

            // Check user exists
            if (user == null)
            {
                return null;
            }
        }

        // Ensure the provider matches
        if (user.ExternalProvider != provider)
        {
            throw new SiteException(ErrorCode.WrongExternalProvider);
        }

        // Create token model
        var token = new AuthToken
        {
            UserId = user.UserId,
            IdentityKey = identity.Key,
            IpAddress = identity.IpAddress,
            PlatformRaw = identity.Platform,
            TwoFactorPassed = true,
            RefreshToken = GenerateRefreshToken()
        };
        token.SetPlatform();

        // Return reponse
        return await _ctx.CreateAsync(token);
    }

    public async Task<AuthToken> TouchIdLogin(Guid key, ClientIdentity identity)
    {
        // Make sure token is valid
        var validToken = await _ctx.AuthTokens.AnyAsync(t => t.AuthTokenId == key && t.TouchIdEnabled && t.IdentityKey == identity.Key && t.IpAddress == identity.IpAddress && t.PlatformRaw == identity.Platform);
        if (!validToken)
        {
            throw new SiteException(ErrorCode.TokenInvalid);
        }

        return await RefreshAuthToken(key, null, false);
    }

    public async Task<AuthToken> RefreshAuthToken(Guid authTokenId, string refreshToken, bool enforce)
    {
        // Find token
        var token = await _ctx.AuthTokens
            .Include(t => t.User)
            .FindOrNullAsync(t => t.AuthTokenId == authTokenId);

        // Verify token exists
        if (token == null)
        {
            throw new SiteException(ErrorCode.TokenInvalid);
        }

        // Verify existance
        if ((token.Deleted && !token.TouchIdEnabled) || token.User == null)
        {
            throw new SiteException(ErrorCode.SessionHasExpired);
        }

        // Verify user status
        EnsureAccountIsEnabled(token.User.Status);

        // Enforce refresh token?
        if (enforce)
        {
            // If the token has been refreshed recently return the same on back
            if (token.RefreshedAt > DateTime.UtcNow.AddSeconds(-5))
            {
                return token;
            }

            // Ensure the refresh token is legit
            if (refreshToken != token.RefreshToken)
            {
                throw new SiteException(ErrorCode.SessionHasExpired);
            }
        }

        // Update tokens data
        token.Updated = DateTime.UtcNow;
        token.RefreshToken = GenerateRefreshToken();
        token.Refreshes++;
        token.RefreshedAt = DateTime.UtcNow;
        token.User.LastActive = DateTime.UtcNow;

        // Has 2FA been disabled?
        if (!token.TwoFactorPassed && token.User.TwoFactorType == TwoFactorType.Unset)
        {
            token.TwoFactorPassed = true;
        }

        return await _ctx.UpdateAsync(token);
    }

    public async Task VerifyAuth(Guid userId, VerifyAuthRequest request)
    {
        // Verify touch id token
        if (request.TouchIdKey.HasValue)
        {
            var validToken = await _ctx.AuthTokens.AnyAsync(t => t.AuthTokenId == request.TouchIdKey && t.TouchIdEnabled);
            if (!validToken)
            {
                throw new SiteException(ErrorCode.TokenInvalid);
            }
        }

        // Verify password property
        else if (!string.IsNullOrEmpty(request.Password))
        {
            await _passwordService.Verify(userId, request.Password);
        }

        // Nothing provided, fail for non-external providers as we can't verify their password
        else if (await _ctx.Users.AnyAsync(u => u.UserId == userId && u.ExternalProvider == ExternalProvider.Unset))
        {
            throw new SiteException(ErrorCode.IncorrectPassword);
        }
    }

    public async Task Logout(Guid authTokenId)
    {
        // Find token
        var token = await _ctx.AuthTokens.FindOrNullAsync(t => t.AuthTokenId == authTokenId);
        if (token?.Deleted != false)
        {
            return;
        }

        // Delete token
        token.Deleted = true;
        token.Updated = DateTime.UtcNow;

        // Update entity in set
        await _ctx.UpdateAsync(token);
    }

    public async Task<AuthToken> ValidateTwoFactorCode(Guid userId, Guid authTokenId, string code, bool doNotAskAgain)
    {
        // Find token
        var token = await _ctx.AuthTokens
            .Include(t => t.User)
            .FindAsync(t => t.AuthTokenId == authTokenId);

        // Verify code
        if (!await _twoFactorService.VerifyCode(userId, code, token.User.TwoFactorSecret, token.User.TwoFactorType))
        {
            throw new SiteException(ErrorCode.ProvidedCodeInvalid);
        }

        // Enable session
        token.Updated = DateTime.UtcNow;
        token.TwoFactorPassed = true;
        token.RememberIdentityForTwoFactor = doNotAskAgain;

        return await _ctx.UpdateAsync(token);
    }

    public async Task ForgetIdentity(Guid userId)
    {
        // Mark tokens as forgotten
        await _ctx.AuthTokens
            .Where(t => t.UserId == userId && t.RememberIdentityForTwoFactor)
            .ExecuteUpdateAsync(prop => prop.SetProperty(p => p.RememberIdentityForTwoFactor, false));
    }

    public async Task<AuthToken> EnableDisableTouchId(Guid authTokenId, ClientIdentity identity, bool enabled)
    {
        // Find token
        var token = await _ctx.AuthTokens.FindAsync(t => t.AuthTokenId == authTokenId && t.IdentityKey == identity.Key && t.IpAddress == identity.IpAddress && t.PlatformRaw == identity.Platform);

        // Mark this token for touch id
        token.TouchIdEnabled = enabled;

        return await _ctx.UpdateAsync(token);
    }

    public async Task<AuthToken> Delete(Guid authTokenId)
    {
        // Find token
        var token = await _ctx.AuthTokens.FindOrNullAsync(t => t.AuthTokenId == authTokenId);
        if (token?.Deleted != false)
        {
            return null;
        }

        // Delete token
        token.Deleted = true;
        token.Updated = DateTime.UtcNow;

        return await _ctx.UpdateAsync(token);
    }

    public async Task DeleteAll(Guid userId)
    {
        // Find token
        var tokens = _ctx.AuthTokens.Where(t => t.UserId == userId && !t.Deleted);
        if (!await tokens.AnyAsync())
        {
            return;
        }

        // Delete tokens
        var updates = tokens.ToList().Select(token =>
        {
            token.Deleted = true;
            token.Updated = DateTime.UtcNow;

            return token;
        });

        await _ctx.UpdateManyAsync(updates);
    }

    private static void EnsureAccountIsEnabled(UserStatus status)
    {
        if (status == UserStatus.Enabled)
        {
            return;
        }

        switch (status)
        {
            case UserStatus.Disabled:
                throw new SiteException(ErrorCode.AccountDisabled);

            case UserStatus.Locked:
                throw new SiteException(ErrorCode.AccountLocked);
        }
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }
}