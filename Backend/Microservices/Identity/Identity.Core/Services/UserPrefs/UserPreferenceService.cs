using Identity.Domain;

using Library.Core;

using Microsoft.EntityFrameworkCore;

namespace Identity.Core.Services;

public class UserPreferenceService : IUserPreferenceService
{
    private readonly IdentityContext _ctx;

    public UserPreferenceService(IdentityContext ctx)
    {
        _ctx = ctx;
    }

    public async Task SetMarketing(Guid userId, bool enabled)
    {
        var prefs = await _ctx.UserPreferences.FindOrNullAsync(u => u.UserId == userId);
        if (prefs == null)
        {
            prefs = new UserPreference(userId, enabled);
            prefs = await _ctx.CreateAsync(prefs);
        }
        else
        {
            prefs.MarketingEmails = enabled;
            await _ctx.UpdateAsync(prefs);
        }
    }

    public async Task UnsubscribeFromMarketing(Guid optOutKey)
    {
        // Check for key
        var userKey = await _ctx.UserKeys
            .Include(k => k.User).ThenInclude(k => k.Preferences)
            .FindAsync(k => k.Key == $"{optOutKey}" && k.Type == UserKeyType.MarketingOptOut && !k.Invalidated);

        // Stop if user already opted out
        if (userKey.User.Preferences?.MarketingEmails == true)
        {
            return;
        }

        // Ensure this hasn't already been redeemed or expired
        if (userKey.UsedAt.HasValue || userKey.Expired)
        {
            throw new SiteException(ErrorCode.KeyExpiredOrInvalid);
        }

        // Update prefs
        userKey.User.Preferences.MarketingEmails = false;
        userKey.UsedAt = DateTime.UtcNow;

        _ctx.UserKeys.Update(userKey);
        _ctx.UserPreferences.Update(userKey.User.Preferences);

        await _ctx.SaveChangesAsync();

    }
}