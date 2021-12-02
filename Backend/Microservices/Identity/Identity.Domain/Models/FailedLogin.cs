using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Identity.Domain;

public class FailedLogin
{
    public FailedLogin()
    {
    }

    public FailedLogin(Guid userId)
    {
        UserId = userId;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid FailedLoginId { get; set; }

    [Required]
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public Guid UserId { get; set; }

    public virtual User User { get; set; }
}