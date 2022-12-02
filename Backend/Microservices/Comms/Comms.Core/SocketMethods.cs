using Comms.Domain.Dtos;

using Library.Core;
using Library.Service.PubSub;

namespace Comms.Core;

public static class SocketMethods
{
    public static Task NewNotification(this ISocketsPubSub sub, Guid userId, NotificationDto notification, UserLevel? userLevel = null) => sub.Send(new("NewNotification", userId, userLevel, notification));

    public static Task UpdatedUserSettings(this ISocketsPubSub sub, Guid userId, UserSettingDto settings) => sub.Send(new("UpdatedUserPreferences", userId, settings));
}
