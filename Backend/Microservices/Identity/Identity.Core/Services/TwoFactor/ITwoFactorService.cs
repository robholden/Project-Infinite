
using Identity.Domain;

namespace Identity.Core.Services;

public interface ITwoFactorService
{
    Task<string> GenerateCode(Guid userId, string secret, TwoFactorType type);

    Task<bool> VerifyCode(Guid userId, string code, string secret, TwoFactorType type);

    Task<IEnumerable<RecoveryCode>> GenerateRecoveryCodes(Guid userId);

    Task Recover(Guid userId, string value);
}