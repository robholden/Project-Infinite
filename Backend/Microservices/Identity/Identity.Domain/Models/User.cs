using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Library.Core;

namespace Identity.Domain;

public class User : IUser
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid UserId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; }

    [Required, MinLength(3), MaxLength(50)]
    public string Username { get; set; }

    public bool EmailConfirmed { get; set; }

    [MaxLength(50), DataType(DataType.PhoneNumber)]
    public string Mobile { get; set; }

    public virtual ICollection<Password> Passwords { get; set; }

    public virtual ICollection<UserKey> UserKeys { get; set; }

    public virtual ICollection<AuthToken> AuthTokens { get; set; }

    public virtual ICollection<RecoveryCode> RecoveryCodes { get; set; }

    public virtual UserPreference Preferences { get; set; }

    public bool TwoFactorEnabled { get; set; }

    [MaxLength(100)]
    public string TwoFactorSecret { get; set; }

    public TwoFactorType TwoFactorType { get; set; }

    public ExternalProvider ExternalProvider { get; set; }

    public string ExternalProviderIdentifier { get; set; }

    public UserLevel UserLevel { get; set; }

    public DateTime? LastActive { get; set; }

    [Required]
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public UserStatus Status { get; set; }
}