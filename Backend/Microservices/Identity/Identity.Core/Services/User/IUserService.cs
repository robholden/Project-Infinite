
using Identity.Domain;

using Library.Core;

namespace Identity.Core.Services;

public interface IUserService
{
    Task<User> Register(RegisterRequest register);

    Task<UserStatus> LoginAttempted(User user, bool pass, ClientIdentity identity);

    Task ForgotPassword(string email);

    Task<string> Setup2FA(Guid userId, TwoFactorType type, string param = null);

    Task Trigger2FA(User user);

    Task Trigger2FA(Guid userId);

    Task<User> Disable2FA(Guid userId);

    Task<User> Enable2FA(Guid userId, string code);

    Task<string> SendEmailConfirmation(Guid userId, bool autoSend = true);

    Task UpdateUsername(Guid userId, string username);

    Task UpdateEmail(Guid userId, string email);

    Task UpdateLastActive(Guid userId);

    Task UpdateMobile(Guid userId, string mobile);

    Task UpdateName(Guid userId, string name);

    Task UpdateLevel(Guid userId, UserLevel level);

    Task<UserStatus> UpdateStatus(Guid userId, UserStatus status);

    Task<User> VerifyAndConfirmEmail(string key);

    Task<User> VerifyAndResetPassword(string key, string password);

    Task<User> DeleteAccount(Guid userId);
}