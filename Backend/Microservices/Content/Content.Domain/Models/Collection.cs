using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Library.Core.Models;

namespace Content.Domain;

public class Collection : IUser
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid CollectionId { get; set; }

    public Guid UserId { get; set; }

    [Required, MinLength(3), MaxLength(50)]
    public string Username { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Picture> Pictures { get; set; }
}