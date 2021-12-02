using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Content.Domain;

public class PictureModeration
{
    public PictureModeration()
    {
    }

    public PictureModeration(Guid pictureId)
    {
        PictureId = pictureId;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid ModerationId { get; set; }

    [Key]
    public Guid PictureId { get; set; }

    public virtual Picture Picture { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public DateTime? LockedAt { get; set; }

    public string LockedBy { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }
}