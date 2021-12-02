using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Library.Core.Enums;
using Library.Core.Models;

using Newtonsoft.Json;

namespace Comms.Domain;

public class Notification : IUser
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid NotificationId { get; set; }

    public Guid UserId { get; set; }

    public UserLevel? UserLevel { get; set; }

    [MinLength(3), MaxLength(50)]
    public string Username { get; set; }

    [MaxLength(255)]
    public string ContentKey { get; set; }

    [MaxLength(255)]
    public string ContentImage { get; set; }

    [MaxLength(255)]
    public string ContentMessage { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public DateTime? ViewedAt { get; set; }

    [NotMapped]
    public bool Viewed => ViewedAt.HasValue;

    public DateTime? ReadAt { get; set; }

    [NotMapped]
    public bool Read => ReadAt.HasValue;

    public NotificationType Type { get; set; }

    public bool Hidden { get; set; }

    public virtual ICollection<NotificationEntry> Entries { get; set; }
}