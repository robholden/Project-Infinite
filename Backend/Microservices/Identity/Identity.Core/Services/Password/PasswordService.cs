
using Identity.Domain;

using Library.Core;

namespace Identity.Core.Services;

public class PasswordService : IPasswordService
{
    private readonly IdentityContext _ctx;

    public PasswordService(IdentityContext ctx)
    {
        _ctx = ctx;
    }

    public async Task Change(Guid userId, string oldPassword, string newPassword)
    {
        // Get current instance
        var pw = await _ctx.Passwords.FindAsync(p => p.UserId == userId);

        // Validate provided password with existing
        if (!pw.Verify(oldPassword))
        {
            throw new SiteException(ErrorCode.IncorrectPassword);
        }

        // Apply new password
        pw.Set(newPassword);
        await _ctx.UpdateAsync(pw);
    }

    public async Task<bool> Check(Guid userId, string password)
    {
        // Get current instance
        var pw = await _ctx.Passwords.FindAsync(p => p.UserId == userId);

        // Validate provided password with existing
        return pw.Verify(password);
    }

    public async Task Set(Guid userId, string text)
    {
        // Get existing password
        var password = await _ctx.Passwords.FindOrNullAsync(p => p.UserId == userId);

        // Create new password?
        if (password == null)
        {
            password = new Password(userId, text);
            await _ctx.CreateAsync(password);
            return;
        }

        password.Set(text);
        await _ctx.UpdateAsync(password);
    }

    public async Task Verify(Guid userId, string password)
    {
        // Verify password
        if (string.IsNullOrEmpty(password) || !await Check(userId, password))
        {
            throw new SiteException(ErrorCode.IncorrectPassword);
        }
    }
}