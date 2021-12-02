using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Library.Core.Enums;
using Library.Core.Models;

namespace Comms.Domain;

public class EmailQueue : IUser
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid EmailQueueId { get; set; }

    public Guid UserId { get; set; }

    public virtual UserSetting User { get; set; }

    [MinLength(3), MaxLength(50)]
    public string Username { get; set; }

    [EmailAddress, MaxLength(255)]
    public string EmailAddress { get; set; }

    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(500)]
    public string IdentityHash { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(500)]
    public string Message { get; set; }

    [Required, MaxLength(255)]
    public string Subject { get; set; }

    public EmailType Type { get; set; }

    public virtual Email Email { get; set; }

    public Guid? OwnedBy { get; set; }

    public bool Completed { get; set; }

    public DateTime? CompletedAt { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }
}