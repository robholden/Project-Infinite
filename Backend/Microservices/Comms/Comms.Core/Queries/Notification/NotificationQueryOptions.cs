
using Library.Core;
using Library.Core.Enums;

namespace Comms.Core.Queries;

public class NotificationQueryOptions : IPageListQuery<NotificationQueryOptions.OrderByEnum>
{
    public enum OrderByEnum
    {
        None,
        Date
    }

    public Guid? UserId { get; set; }

    public List<UserLevel> UserLevels { get; set; } = new();

    public bool? Viewed { get; set; }

    public bool? Read { get; set; }

    public List<NotificationType> Types { get; set; } = new();

    public string AfterTimestamp { get; set; }

    public OrderByEnum OrderBy { get; set; }
}