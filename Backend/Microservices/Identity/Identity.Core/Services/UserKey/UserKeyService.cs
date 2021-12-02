using System.Data;

using Identity.Domain;

using Library.Core;

using Microsoft.EntityFrameworkCore;

namespace Identity.Core.Services;

public class UserKeyService : IUserKeyService
{
    private readonly IdentityContext _ctx;

    public UserKeyService(IdentityContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<UserKey> Create(Guid userId, UserKeyType type, DateTime? expires, string key = null)
    {
        // Create new key model
        var userKey = new UserKey(userId, type, expires, key);

        // Get old keys
        var time = DateTime.UtcNow;
        var oldKeys = _ctx.UserKeys.Where(
            k => k.UserId == userId &&
            k.Type == type &&
            !k.UsedAt.HasValue &&
            (!k.Expires.HasValue || k.Expires.Value < DateTime.UtcNow) &&
            !k.Invalidated
        );

        // Invalidate old keys
        if (await oldKeys.AnyAsync())
        {
            await oldKeys.ForEachAsync(k => k.Invalidated = true);
            await _ctx.PutRange(oldKeys);
        }

        // Add and return entity
        return await _ctx.Put(userKey);
    }


    public async Task InvalidateKeys(Guid userId, UserKeyType type)
    {
        // Get keys to invalidate
        var oldKeys = _ctx.UserKeys.Where(k => k.UserId == userId && k.Type == type && !k.Invalidated);

        // Invalidate old keys
        if (!await oldKeys.AnyAsync())
        {
            return;
        }

        await oldKeys.ForEachAsync(k => k.Invalidated = true);
        await _ctx.PutRange(oldKeys);
    }

    public async Task<UserKey> UseKey(string key, UserKeyType type)
    {
        // Get key instance
        var userKey = await _ctx.UserKeys
            .Include(k => k.User)
            .FindOrNullAsync(k => k.Key == key && k.Type == type && !k.UsedAt.HasValue);

        if (userKey == null)
        {
            return null;
        }

        // Redeem key
        userKey.UsedAt = DateTime.UtcNow;
        return await _ctx.Put(userKey);
    }

    public async Task<UserKey> ValidateAndUseKey(string key, UserKeyType type)
    {
        await ValidateKey(key, type);
        return await UseKey(key, type);
    }

    public async Task ValidateKey(string key, UserKeyType type)
    {
        // Get key instance
        var userKey = await _ctx.UserKeys.FindAsync(k => k.Key == key && k.Type == type, ErrorCode.ProvidedKeyInvalid);

        // Ensure this hasn't already been redeemed or expired
        if (userKey.UsedAt.HasValue || userKey.Expired)
        {
            throw new SiteException(ErrorCode.KeyExpiredOrInvalid);
        }
    }

    public Task<UserKey> ExistingKey(Guid userId, UserKeyType type)
    {
        return _ctx.UserKeys.FirstOrDefaultAsync(k => k.UserId == userId && k.Type == type && !k.UsedAt.HasValue && !k.Invalidated && (!k.Expires.HasValue || k.Expires.Value < DateTime.UtcNow));
    }
}