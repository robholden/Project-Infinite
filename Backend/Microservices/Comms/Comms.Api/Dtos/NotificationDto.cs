
using Library.Core.Enums;

namespace Comms.Api.Dtos;

public class NotificationDto
{
    public Guid NotificationId { get; set; }

    public string ContentKey { get; set; }

    public string ContentMessage { get; set; }

    public string ContentImage { get; set; }

    public int Users { get; set; }

    public DateTime Date { get; set; }

    public bool Viewed { get; set; }

    public bool Read { get; set; }

    public NotificationType Type { get; set; }

    public bool IsGlobal { get; set; }
}