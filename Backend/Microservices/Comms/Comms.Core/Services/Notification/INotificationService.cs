using Comms.Domain;
using Comms.Domain.Dtos;

using Library.Core;

namespace Comms.Core.Services;

public interface INotificationService
{
    Task MarkAllAsRead(Guid userId, UserLevel? level = null);

    Task MarkAsRead(Guid id);

    Task Viewed(Guid userId, UserLevel? level = null);

    Task Delete(Guid id);

    Task<bool> TryToSend(Notification notification, NotificationDto payload);
}