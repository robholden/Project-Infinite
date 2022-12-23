using Library.Core;

namespace Comms.Domain.Dtos;

public class NotificationDto
{
    public Guid NotificationId { get; set; }

    public string Identifier { get; set; }

    public NotificationType Type { get; set; }

    public string ContentRoute { get; set; }

    public string ContentImageUrl { get; set; }

    public int Users { get; set; }

    public DateTime Date { get; set; }

    public bool Viewed { get; set; }

    public bool Read { get; set; }

    public bool IsGlobal { get; set; }
}