using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Library.Core;

namespace Content.Domain;

public class PictureLike : IUser
{
    public PictureLike()
    {
    }

    public PictureLike(Guid pictureId, Guid userId, string username)
    {
        PictureId = pictureId;
        UserId = userId;
        Username = username;
    }

    public PictureLike(Guid pictureId, IUser user) : this(pictureId, user.UserId, user.Username)
    {
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid PictureLikeId { get; set; }

    [Key]
    public Guid UserId { get; set; }

    [Key]
    public Guid PictureId { get; set; }

    [Required, MinLength(3), MaxLength(50)]
    public string Username { get; set; }

    public virtual Picture Picture { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}