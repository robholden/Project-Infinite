using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Identity.Domain;

public class UserKey
{
    public UserKey()
    {
    }

    public UserKey(Guid userId, UserKeyType type, DateTime? expires, string key = null)
    {
        UserId = userId;
        Type = type;
        Expires = expires;
        Key = string.IsNullOrEmpty(key) ? Guid.NewGuid().ToString() : key;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid UserKeyId { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public bool Expired => Expires <= DateTime.UtcNow;

    public DateTime? Expires { get; set; }

    public bool Invalidated { get; set; }

    [Required]
    public string Key { get; set; }

    [Required]
    public UserKeyType Type { get; set; }

    public DateTime? UsedAt { get; set; }

    public Guid UserId { get; set; }

    public virtual User User { get; set; }
}