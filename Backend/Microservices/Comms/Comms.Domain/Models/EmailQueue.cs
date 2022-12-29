using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Library.Core;
using Library.Service.PubSub;

namespace Comms.Domain;

public class EmailQueue : IUser
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid EmailQueueId { get; set; }

    public Guid UserId { get; set; }

    [MinLength(3), MaxLength(50)]
    public string Username { get; set; }

    [Required, EmailAddress, MaxLength(255)]
    public string EmailAddress { get; set; }

    [MaxLength(100)]
    public string Name { get; set; }

    public bool Sendable { get; set; }

    [MaxLength(500)]
    public string IdentityHash { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(500)]
    public string Message { get; set; }

    [Required, MaxLength(255)]
    public string Subject { get; set; }

    public virtual Email Email { get; set; }

    public Guid? OwnedBy { get; set; }

    public bool Completed { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string OptOutKey { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }

    public static EmailQueue FromUserReq(SendEmailToUserRq request) => request == null ? null : new()
    {
        EmailAddress = request.Email,
        Message = request.Message,
        Subject = request.Subject,
        UserId = request.User.UserId,
        Username = request.User.Username,
        IdentityHash = string.IsNullOrEmpty(request.IdentityHash) ? request.Subject : request.IdentityHash,
        Sendable = request.SendInstantly
    };
}