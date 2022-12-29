using Identity.Domain;

using Library.Core;
using Library.Service.PubSub;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using OtpNet;

namespace Identity.Core.Services;

public class UserService : IUserService
{
    private readonly IdentityContext _ctx;
    private readonly IdentitySettings _settings;

    private readonly IUserKeyService _keyService;
    private readonly IPasswordService _passwordService;
    private readonly ITwoFactorService _twoFactorService;

    private readonly IIdentityPubSub _identityEvents;
    private readonly ICommsPubSub _commEvents;
    private readonly ISocketsPubSub _socketEvents;

    public UserService(
        IdentityContext ctx,
        IOptions<IdentitySettings> options,
        IUserKeyService keyService,
        IPasswordService passwordService,
        ITwoFactorService twoFactorService,
        IIdentityPubSub identityEvents,
        ICommsPubSub commEvents,
        ISocketsPubSub socketEvents
    )
    {
        _ctx = ctx;
        _settings = options.Value;
        _keyService = keyService;
        _passwordService = passwordService;
        _twoFactorService = twoFactorService;

        _identityEvents = identityEvents;
        _commEvents = commEvents;
        _socketEvents = socketEvents;
    }

    public async Task<User> Register(RegisterRequest register)
    {
        // Get user from registration
        var user = new User
        {
            Username = register.Username,
            Name = register.Name,
            Email = register.Email,
            Mobile = register.Mobile,
            UserLevel = new string[] { "rob", "admin" }.Contains(register.Username.ToLower()) ? UserLevel.Admin : UserLevel.Default
        };

        // Is this an external provider registration?
        if (register.ExternalProvider != null)
        {
            user.ExternalProvider = register.ExternalProvider.Provider;
            user.ExternalProviderIdentifier = register.ExternalProvider.UserIdentifier;
            user.EmailConfirmed = true;
        }
        else
        {
            var password = new Password(user.UserId, register.Password);
            user.Passwords = new List<Password>() { password };
        }

        // Validate properties
        await ValidateUsername(user.Username);
        await ValidateEmail(user.Email);

        // Add user to db
        user = await _ctx.CreateAsync(user);

        // Send email confirmation
        if (user.ExternalProvider == ExternalProvider.Unset && !user.EmailConfirmed)
        {
            await SendEmailConfirmation(user.UserId);
        }

        return user;
    }

    public async Task<UserStatus> LoginAttempted(User user, bool pass, ClientIdentity identity)
    {
        if (!pass)
        {
            await _ctx.CreateAsync(new FailedLogin(user.UserId));
        }

        // Lockout user if they've made too many failed attempts
        var attempts = await _ctx.FailedLogins.CountAsync(l => l.UserId == user.UserId && l.Created > DateTime.UtcNow.AddMinutes(-_settings.FailedLoginDuration));
        if (attempts > _settings.FailedLoginAttempts)
        {
            return await UpdateStatus(user.UserId, UserStatus.Locked);
        }

        // Unlock account
        var status = user.Status;
        if (user.Status == UserStatus.Locked)
        {
            status = await UpdateStatus(user.UserId, UserStatus.Enabled);
        }

        // Notify user that a new location has logged in
        var known = await _ctx.AuthTokens.AnyAsync(t => t.UserId == user.UserId && t.IpAddress == identity.IpAddress && t.Created > DateTime.UtcNow.AddMonths(-6));
        if (!known)
        {
            var body = @$"
                This email was generated because a new log-in has occurred for the account {user.Username} on {DateTime.UtcNow:MMMM dd, yyyy hh:mm:ss t} UTC originating from:
                    •	Platform: {identity.Platform}
                    •	IP address: {identity.IpAddress}
            ";
            var email = new SendEmailToUserRq(user.ToUserRecord(), user.Email, body, "Successful Log-in", true, identity.Key);
            _ = _commEvents?.AddNotification(new(user.ToUserRecord(), identity.IpAddress, NotificationType.NewLogin, Content: new(), Email: email));
        }

        return status;
    }

    public async Task ForgotPassword(string email)
    {
        // Fetch user
        var user = await _ctx.Users.FindAsync(u => u.Email.ToLower() == email.ToLower().Trim());

        // Find existing key
        var userKey = await _keyService.ExistingKey(user.UserId, UserKeyType.PasswordReset);

        // Only send link once per minute
        if (userKey?.Created < DateTime.UtcNow.AddMinutes(-1))
        {
            return;
        }

        // Create a new one
        userKey ??= await _keyService.Create(user.UserId, UserKeyType.PasswordReset, DateTime.UtcNow.AddDays(1));

        // Email the verification to user
        var subject = "Password Reset Link";
        var message = $@"
                <p>You've requested a password reset. Please click this link to reset your password (it will expire after 24 hours):</p>
                <p>###SITE_URL###/reset-password/{userKey.Key}</p>
            ";
        _ = _commEvents?.SendEmailToUser(new(user.ToUserRecord(), user.Email, message, subject));
    }

    public async Task<string> Setup2FA(Guid userId, TwoFactorType type, string param = null)
    {
        // Fetch user
        var user = await _ctx.Users.FindAsync(u => u.UserId == userId);

        // Ensure we have a valid type
        if (type == TwoFactorType.Unset)
        {
            throw new SiteException(ErrorCode.TwoFactorTypeUnset);
        }

        // Ensure the user has confirmed their email
        if (!user.EmailConfirmed)
        {
            throw new SiteException(ErrorCode.EmailConfirmationRequired);
        }

        // Ensure they're not setup already
        if (user.TwoFactorEnabled)
        {
            throw new SiteException(ErrorCode.TwoFactorAlreadyEnabled);
        }

        // Add number to user
        if (type == TwoFactorType.SMS)
        {
            user.Mobile = param;
        }

        // Set 2FA properties
        user.TwoFactorType = type;
        user.TwoFactorSecret = type == TwoFactorType.App ? Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(_settings.Totp.Mode)) : null;

        // Trigger 2FA
        await Trigger2FA(user);

        // Now save the user
        user = await _ctx.UpdateAsync(user);

        return user.TwoFactorSecret;
    }

    public async Task Trigger2FA(Guid userId)
    {
        var user = await _ctx.Users.FindAsync(u => u.UserId == userId);
        await Trigger2FA(user);
    }

    public async Task Trigger2FA(User user)
    {
        // Stop if type is app
        if (user == null || user.TwoFactorType == TwoFactorType.App)
        {
            return;
        }

        // Run send logic
        var code = await _twoFactorService.GenerateCode(user.UserId, user.TwoFactorSecret, user.TwoFactorType);
        switch (user.TwoFactorType)
        {
            case TwoFactorType.Email:
                string body, subject;

                if (user.TwoFactorEnabled)
                {
                    subject = "Your two-factor sign in code";
                    body = $"<p>You recently tried to login from a new device, browser, or location. In order to complete your login, please use the following code:</p> {code}";
                }
                else
                {
                    subject = "Your two-factor setup code";
                    body = $"<p>You recently tried to setup email two-factor authentication. In order to complete your setup, please use the following code:</p> {code}";
                }

                _ = _commEvents?.SendEmailToUser(new(user.ToUserRecord(), user.Email, body, subject, true));
                break;

            case TwoFactorType.SMS:
                var message = $"Your Project Infinite one time password is {code}";
                _ = _commEvents?.SendSmsToUser(new(user.ToUserRecord(), "2fa", user.Mobile, message));

                break;
        }
    }

    public async Task<User> Disable2FA(Guid userId)
    {
        // Fetch user
        var user = await _ctx.Users.FindAsync(u => u.UserId == userId);

        // Ensure they're not setup already
        if (!user.TwoFactorEnabled)
        {
            return user;
        }

        // Reset 2FA properties
        user.TwoFactorEnabled = false;
        user.TwoFactorSecret = null;
        user.TwoFactorType = TwoFactorType.Unset;

        // Update db
        user = await _ctx.UpdateAsync(user);

        // Forget token identity
        await _ctx.ExecuteUpdateAsync<AuthToken>(
            t => t.UserId == userId && t.RememberIdentityForTwoFactor,
            entry => entry.RememberIdentityForTwoFactor = false
        );

        // Send email to user for confirmation
        var subject = "You've disabled two-factor authentication";
        var message = "<p>We're informing you that you have successfully disabled two-factor authentication.</p>";
        _ = _commEvents?.SendEmailToUser(new(user.ToUserRecord(), user.Email, message, subject));

        return user;
    }

    public async Task<User> Enable2FA(Guid userId, string code)
    {
        // Fetch user
        var user = await _ctx.Users.FindAsync(u => u.UserId == userId);

        // Ensure they're not setup already
        if (user.TwoFactorEnabled)
        {
            throw new SiteException(ErrorCode.TwoFactorAlreadyEnabled);
        }

        // Verify provided code
        var passed = await _twoFactorService.VerifyCode(userId, code, user.TwoFactorSecret, user.TwoFactorType);
        if (!passed)
        {
            throw new SiteException(ErrorCode.ProvidedCodeInvalid);
        }

        // Set 2FA properties
        user.TwoFactorEnabled = true;
        user = await _ctx.UpdateAsync(user);

        // Send email to user for confirmation
        var subject = "You've enabled two-factor authentication";
        var message = "<p>We're informing you that you have successfully setup two-factor authentication.</p>";
        _ = _commEvents?.SendEmailToUser(new(user.ToUserRecord(), user.Email, message, subject));

        return user;
    }

    public async Task<string> SendEmailConfirmation(Guid userId, bool autoSend = true)
    {
        // Fetch user
        var user = await _ctx.Users.FindAsync(u => u.UserId == userId);

        // Invalidate any keys that have be used
        await _keyService.InvalidateKeys(userId, UserKeyType.ConfirmEmail);

        // Reset email confirm flag
        if (user.EmailConfirmed)
        {
            user.EmailConfirmed = false;
            user = await _ctx.UpdateAsync(user);
        }

        // Create confirmation key
        var userKey = await _keyService.Create(user.UserId, UserKeyType.ConfirmEmail, null);

        // Email the verification to user
        if (autoSend)
        {
            var subject = "Email Confirmation";
            var message = $@"
                <p>You've recently signed up to ###SITE_NAME###.</p>
                <p>To complete the registration process, please confirm your email address by clicking the link below:</p>
                <p>###SITE_URL###/confirm-email/{userKey.Key}</p>
            ";
            _ = _commEvents?.SendEmailToUser(new(user.ToUserRecord(), user.Email, message, subject));
        }

        return userKey.Key;
    }

    public async Task UpdateLastActive(Guid userId)
    {
        // Fetch user
        var user = await _ctx.Users.FindAsync(u => u.UserId == userId);

        user.LastActive = DateTime.UtcNow;
        await _ctx.UpdateAsync(user);
    }

    public async Task UpdateUsername(Guid userId, string username)
    {
        // Fetch user
        var user = await _ctx.Users.FindAsync(u => u.UserId == userId);

        // Check is username is new
        if (user.Username == username)
        {
            return;
        }

        // Ensure email is unique
        await ValidateUsername(username);

        // Update email property
        user.Username = username;
        user = await _ctx.UpdateAsync(user);

        // Tell ALL microservices of this change
        _ = _identityEvents?.UpdatedUsername(new(user.UserId, user.Username));
    }

    public async Task UpdateEmail(Guid userId, string email)
    {
        // Fetch user
        var user = await _ctx.Users.FindAsync(u => u.UserId == userId);

        // Check is email is new
        if (user.Email == email)
        {
            return;
        }

        // Ensure email is unique
        await ValidateEmail(email);

        // Update email property
        user.Email = email;
        user = await _ctx.UpdateAsync(user);

        // Tell microservices of this change
        _ = _identityEvents?.UpdatedEmail(new(user.UserId, user.Email));

        // Send email confirmation?
        await SendEmailConfirmation(user.UserId);
    }

    public async Task UpdateMobile(Guid userId, string mobile)
    {
        // Fetch user
        var user = await _ctx.Users.FindAsync(u => u.UserId == userId);

        // Update mobile property
        user.Mobile = mobile;
        await _ctx.UpdateAsync(user);
    }

    public async Task UpdateName(Guid userId, string name)
    {
        // Fetch user
        var user = await _ctx.Users.FindAsync(u => u.UserId == userId);

        // Update name property
        user.Name = name;
        await _ctx.UpdateAsync(user);
    }

    public async Task<UserStatus> UpdateStatus(Guid userId, UserStatus status)
    {
        // Fetch user
        var user = await _ctx.Users.FindAsync(u => u.UserId == userId);

        // Update name property
        user.Status = status;
        await _ctx.UpdateAsync(user);

        return status;
    }

    public async Task UpdateLevel(Guid userId, UserLevel level)
    {
        // Fetch user
        var user = await _ctx.Users.FindAsync(u => u.UserId == userId);

        // Update name property
        user.UserLevel = level;
        await _ctx.UpdateAsync(user);
    }

    public async Task<User> VerifyAndConfirmEmail(string key)
    {
        // Verify and consume key
        var userKey = await _keyService.ValidateAndUseKey(key, UserKeyType.ConfirmEmail);

        // Update confirmed flag property
        userKey.User.EmailConfirmed = true;
        return await _ctx.UpdateAsync(userKey.User);
    }

    public async Task<User> VerifyAndResetPassword(string key, string password)
    {
        // Verify and consume key
        var userKey = await _keyService.ValidateAndUseKey(key, UserKeyType.PasswordReset);

        // Reset user's password
        await _passwordService.Set(userKey.UserId, password);

        // Send email about change
        var subject = "Password Reset";
        var message = "<p>You've successfully changed your password.</p>";
        _ = _commEvents?.SendEmailToUser(new(userKey.User.ToUserRecord(), userKey.User.Email, message, subject));

        return userKey.User;
    }

    public async Task<User> DeleteAccount(Guid userId)
    {
        // Ensure user exists before deleting
        var user = await _ctx.Users.FirstOrDefaultAsync(p => p.UserId == userId);
        if (user == null) return null;

        // Remove all records of this user
        await _ctx.RemoveAsync(user);

        // Publish delete user event
        await _identityEvents?.DeletedUser(new(userId));

        // Logout all sessions
        _ = _socketEvents?.RevokeSession(user.UserId);

        return user;
    }

    private async Task ValidateUsername(string username)
    {
        // Ensure format is valid
        if (!StringExtensions.UsernameRegex().IsMatch(username))
        {
            throw new SiteException(ErrorCode.InvalidUsername);
        }

        // Invalid users
        username = username.ToLower().Trim();
        if (username.IsAnInvalidUsername())
        {
            throw new SiteException(ErrorCode.InvalidUsername);
        }

        // Check username is unique
        if (await _ctx.Users.FindOrNullAsync(u => u.Username.ToLower() == username) != null)
        {
            throw new SiteException(ErrorCode.UsernameTaken);
        }
    }

    private async Task ValidateEmail(string email)
    {
        if (await _ctx.Users.FindOrNullAsync(u => u.Email.ToLower() == email.ToLower().Trim()) != null)
        {
            throw new SiteException(ErrorCode.EmailInUse);
        }
    }
}