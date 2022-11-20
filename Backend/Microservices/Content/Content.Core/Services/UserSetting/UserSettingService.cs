
using Content.Domain;

using Library.Core;

namespace Content.Core.Services;

public class UserSettingService : IUserSettingService
{
    private readonly ContentContext _ctx;

    public UserSettingService(ContentContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<UserSetting> AddOrUpdate(Guid userId, UserSetting userData)
    {
        // Get current settings
        var (user, exists) = await _ctx.UserSettings.FindWithDefaultAsync(x => x.UserId == userId, new(userId));

        // Update values
        user.MaxPictureSize = userData.MaxPictureSize;
        user.MinPictureResolutionX = userData.MinPictureResolutionX;
        user.MinPictureResolutionY = userData.MinPictureResolutionY;
        user.DraftLimit = userData.DraftLimit;
        user.DailyUploadLimit = userData.DailyUploadLimit;
        user.UploadEnabled = userData.UploadEnabled;

        // Update
        return exists ? await _ctx.UpdateAsync(user) : await _ctx.CreateAsync(user);
    }
}