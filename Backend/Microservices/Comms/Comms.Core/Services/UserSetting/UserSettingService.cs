
using Comms.Domain;

using Library.Core;

namespace Comms.Core.Services;

public class UserSettingService : IUserSettingService
{
    private readonly CommsContext _ctx;

    public UserSettingService(CommsContext ctx)
    {
        _ctx = ctx;
    }

    public async Task Unsubscribe(Guid key)
    {
        // Get user by opt out key
        var user = await _ctx.UserSettings.FindAsync(x => x.MarketingOptOutKey == key, ErrorCode.ProvidedKeyInvalid);

        // Unset values
        user.MarketingEmail = false;
        user.MarketingOptOutKey = null;

        // Update
        await _ctx.Put(user);
    }

    public async Task Update(Guid userId, UserSetting userData)
    {
        // Get user by opt out key
        var user = await _ctx.UserSettings.FindAsync(x => x.UserId == userId);
        if (user == null)
        {
            return;
        }

        // Update values
        if (user.MarketingEmail != userData.MarketingEmail)
        {
            user.MarketingEmail = userData.MarketingEmail;
            user.MarketingOptOutKey = user.MarketingEmail ? Guid.NewGuid() : null;
        }
        user.PictureApprovedEmail = userData.PictureApprovedEmail;
        user.PictureLikedEmail = userData.PictureLikedEmail;
        user.PictureUnapprovedEmail = userData.PictureUnapprovedEmail;

        // Update
        await _ctx.Put(user);
    }
}