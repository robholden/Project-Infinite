using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Library.Core;
using Library.Core.Models;

namespace Content.Domain;

public enum PictureStatus
{
    Draft,
    PendingApproval,
    Published
}

public class Picture : Coords, IUser
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid PictureId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    public Guid UserId { get; set; }

    [Required, MinLength(3), MaxLength(50)]
    public string Username { get; set; }

    [Required, MaxLength(255)]
    public string IpAddress { get; set; }

    [MaxLength(500)]
    public string Colours { get; set; }

    [Required]
    public DateTime DateTaken { get; set; }

    [Required]
    public string Format { get; set; }

    [Required]
    public string Hash { get; set; }

    [Required]
    public int Width { get; set; }

    [Required]
    public int Height { get; set; }

    public bool ConcealCoords { get; set; }

    public virtual Location Location { get; set; }

    [Required, MaxLength(10)]
    public string Ext { get; set; }

    [NotMapped]
    public string Path => $"{ PictureId }{ Ext }";

    [NotMapped]
    public string Seed { get; set; }

    public string SeedKey { get; set; } = Guid.NewGuid().ToString();

    public PictureStatus Status { get; set; }

    public bool Featured { get; set; }

    public virtual ICollection<Tag> Tags { get; set; }

    public virtual ICollection<PictureLike> Likes { get; set; }

    [NotMapped]
    public int LikesTotal { get; set; }

    public virtual ICollection<Collection> Collections { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedDate { get; set; }

    public Picture Seeded()
    {
        Seed = AES.Encrypt(new Seed() { UpdatedDate = UpdatedDate, CreatedDate = DateTime.UtcNow }, SeedKey);
        return this;
    }

    public Seed GetSeed(string seedStr = null)
    {
        return AES.Decrypt<Seed>(seedStr ?? Seed, SeedKey);
    }

    public bool IsSeedValid(string seedStr = null)
    {
        var seed = GetSeed(seedStr);
        if (seed == null) return false;
        if (seed.UpdatedDate != UpdatedDate) return false;
        if (seed.CreatedDate < DateTime.UtcNow.AddDays(-1)) return false;

        return true;
    }
}