using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Identity.Domain;

public class RecoveryCode
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid RecoveryCodeId { get; set; }

    public Guid UserId { get; set; }

    public virtual User User { get; set; }

    public DateTime? UsedAt { get; set; }

    [Required, MaxLength(500)]
    public string Value { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;
}