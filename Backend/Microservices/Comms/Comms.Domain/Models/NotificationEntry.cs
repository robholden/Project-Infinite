using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Library.Core;

namespace Comms.Domain;

public class NotificationEntry : IUser
{
    public NotificationEntry()
    {
    }

    public NotificationEntry(Guid userId, string username)
    {
        UserId = userId;
        Username = username;
    }

    public NotificationEntry(Guid notificationId, Guid userId, string username) : this(userId, username)
    {
        NotificationId = notificationId;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid EntryId { get; set; }

    public Guid UserId { get; set; }

    [Required, MinLength(3), MaxLength(50)]
    public string Username { get; set; }

    public Guid NotificationId { get; set; }

    public virtual Notification Notification { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public bool Sent { get; set; }

    public bool Deleted { get; set; }
}