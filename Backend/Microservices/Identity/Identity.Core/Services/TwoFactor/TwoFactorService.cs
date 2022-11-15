
using Identity.Domain;

using Library.Core;
using Library.Service.PubSub;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using OtpNet;

namespace Identity.Core.Services;

public class TwoFactorService : ITwoFactorService
{
    private readonly IdentityContext _ctx;
    private readonly IdentitySettings _settings;
    private readonly IUserKeyService _keyService;

    private readonly ICommsPubSub _commEvents;

    public TwoFactorService(IdentityContext ctx, IOptions<IdentitySettings> settings, IUserKeyService keyService, ICommsPubSub commEvents)
    {
        _ctx = ctx;
        _settings = settings.Value;
        _keyService = keyService;

        _commEvents = commEvents;
    }

    public async Task<string> GenerateCode(Guid userId, string secret, TwoFactorType type)
    {
        // Generate totp code
        var totp = new Totp(
            string.IsNullOrEmpty(secret) ? KeyGeneration.GenerateRandomKey(20) : Base32Encoding.ToBytes(secret),
            step: _settings.Totp.Step,
            totpSize: _settings.Totp.Size,
            mode: _settings.Totp.Mode
        );
        var code = totp.ComputeTotp();

        if (type != TwoFactorType.App)
        {
            await _keyService.Create(userId, UserKeyType.TwoFactor, DateTime.UtcNow.AddSeconds(_settings.Totp.ExpiresInSeconds), code.ToHash());
        }

        return code;
    }

    public async Task<bool> VerifyCode(Guid userId, string code, string secret, TwoFactorType type)
    {
        // Ensure we haven't already added this code
        if (await _ctx.TwoFactors.AnyAsync(c => c.UserId == userId && c.Code == code))
        {
            throw new SiteException(ErrorCode.CannotReUseCode);
        }

        // Verify code
        bool passed = false;
        long timestamp = 0;

        // If not app, set user key to used
        if (type == TwoFactorType.App)
        {
            passed = new Totp(
                Base32Encoding.ToBytes(secret),
                step: _settings.Totp.Step,
                totpSize: _settings.Totp.Size,
                mode: _settings.Totp.Mode
            ).VerifyTotp(code, out timestamp);
        }
        else
        {
            passed = (await _keyService.UseKey(code.ToHash(), UserKeyType.TwoFactor)) != null;
        }

        // Stop if we've failed
        if (!passed)
        {
            return false;
        }

        // Log 2FA request
        var twoFA = new TwoFactor
        {
            UserId = userId,
            Code = code,
            Type = type,
            TimeStamp = timestamp
        };

        // Add entity to set
        await _ctx.CreateAsync(twoFA);

        return true;
    }

    public async Task<IEnumerable<RecoveryCode>> GenerateRecoveryCodes(Guid userId)
    {
        // Generate new codes
        var codes = new List<RecoveryCode>();
        var codeMap = new List<string>();
        for (var i = 0; i < 10; i++)
        {
            var value = 10.ToRandom();
            codeMap.Add(value);
            codes.Add(
                new RecoveryCode()
                {
                    Value = value.ToHash(),
                    UserId = userId
                }
            );
        }

        // Wrap db stuff in a transaction
        using var transaction = await _ctx.Database.BeginTransactionAsync();

        // Remove existing codes
        await _ctx.RecoveryCodes
            .Where(r => r.UserId == userId && !r.UsedAt.HasValue)
            .ExecuteDeleteAsync();

        // Add new codes
        await _ctx.CreateManyAsync(codes);

        // Commit to db
        await transaction.CommitAsync();

        // Convert hashed values back to normal
        return codes.Select((c, i) =>
        {
            c.Value = codeMap[i];
            return c;
        });
    }

    public async Task Recover(Guid userId, string value)
    {
        // Get given code
        var hash = value.ToHash();
        var user = await _ctx.Users.FindAsync(u => u.UserId == userId, ErrorCode.EntityNotFound);
        var code = await _ctx.RecoveryCodes.FindAsync(r => r.Value == hash && r.UserId == userId && !r.UsedAt.HasValue, ErrorCode.ProvidedCodeInvalid);

        // Wrap db stuff in a transaction
        using var transaction = await _ctx.Database.BeginTransactionAsync();

        // Delete codes we haven't used
        await _ctx.RecoveryCodes
            .Where(r => r.UserId == userId && !r.UsedAt.HasValue && r.RecoveryCodeId != code.RecoveryCodeId)
            .ExecuteDeleteAsync();

        // Update code to used
        code.UsedAt = DateTime.UtcNow;
        await _ctx.UpdateAsync(code);

        // Remove 2FA from user account
        user.TwoFactorEnabled = false;
        user.TwoFactorSecret = null;
        user.TwoFactorType = TwoFactorType.Unset;
        user.LastActive = DateTime.UtcNow;

        // Update user
        await _ctx.UpdateAsync(user);

        // Commit to db
        await transaction.CommitAsync();

        // Send email to user for confirmation
        var subject = "Your account has been recovered";
        var message = "<p>We're informing you that you have successfully recovered your account and disabled your two-factor authentication.</p>";
        _ = _commEvents?.SendEmailToUser(new(user.ToUserRecord(), message, subject, EmailType.System));
    }
}