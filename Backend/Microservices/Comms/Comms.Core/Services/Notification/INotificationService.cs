using Comms.Domain;

using Library.Core.Enums;

namespace Comms.Core.Services;

public interface INotificationService
{
    Task MarkAllAsRead(Guid userId, UserLevel? level = null);

    Task MarkAsRead(Guid id);

    Task Viewed(Guid userId, UserLevel? level = null);

    Task Delete(Guid id);

    Task<bool> TryToSend<T>(Notification notification, T notificationDto);

    Task Send(Notification notification);
}