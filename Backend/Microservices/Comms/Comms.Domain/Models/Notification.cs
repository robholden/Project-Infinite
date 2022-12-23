using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Library.Core;

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

    [MaxLength(50)]
    [Required]
    public string Identifier { get; set; }

    [Required]
    public NotificationType Type { get; set; }

    [MaxLength(255)]
    public string ContentRoute { get; set; }

    [MaxLength(255)]
    public string ContentImageUrl { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public DateTime? ViewedAt { get; set; }

    [NotMapped]
    public bool Viewed => ViewedAt.HasValue;

    public DateTime? ReadAt { get; set; }

    [NotMapped]
    public bool Read => ReadAt.HasValue;

    public bool Hidden { get; set; }

    public Guid? EmailQueueId { get; set; }

    public virtual EmailQueue EmailQueue { get; set; }

    public int Delay { get; set; }

    public virtual ICollection<NotificationEntry> Entries { get; set; }
}