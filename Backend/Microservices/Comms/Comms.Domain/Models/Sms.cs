using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Library.Core;

namespace Comms.Domain;

public class Sms : IUser
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid SmsId { get; set; }

    public Guid UserId { get; set; }

    [Required, MinLength(3), MaxLength(50)]
    public string Username { get; set; }

    [Required, MaxLength(200)]
    public string Message { get; set; }

    [MaxLength(255)]
    public string Mobile { get; set; }

    public DateTime? DateSent { get; set; }

    public bool Sent { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow;
}