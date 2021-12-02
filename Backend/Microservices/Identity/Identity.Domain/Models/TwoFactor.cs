using System.ComponentModel.DataAnnotations;

namespace Identity.Domain;

public class TwoFactor
{
    [Key]
    public int TwoFactorId { get; set; }

    [Required]
    public string Code { get; set; }

    [Required]
    public long? TimeStamp { get; set; }

    public Guid UserId { get; set; }

    public virtual User User { get; set; }

    [Required]
    public TwoFactorType Type { get; set; }
}