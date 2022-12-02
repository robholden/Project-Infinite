using Content.Domain.Dtos;

using Library.Core;
using Library.Service.PubSub;

namespace Content.Core;

public static class SocketMethods
{
    public static Task UpdatedUserSettings(this ISocketsPubSub sub, Guid userId, UserSettingDto settings) => sub.Send(new("UpdatedUserSettings", userId, settings));

    public static Task ModeratedPicture(this ISocketsPubSub sub, Guid pictureId, bool approved) => sub.Send(new("ModeratedPicture", UserLevel.Moderator, new { pictureId, approved }));

    public static Task NewLocation(this ISocketsPubSub sub, Guid userId, Guid pictureId, string name, string country, decimal lat, decimal lng)
    {
        var location = new
        {
            name,
            country = new { name = country },
            lat,
            lng
        };
        return sub.Send(new("NewPictureLocation", userId, new { pictureId = pictureId, location }));
    }
}
