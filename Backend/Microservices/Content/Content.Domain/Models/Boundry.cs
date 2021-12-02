using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Content.Domain;

public static class BoundryExtensions
{
    public static bool Equals(this Boundry src, Boundry dest)
    {
        return src.MinLat == dest.MinLat && src.MaxLat == dest.MaxLat && src.MinLng == dest.MinLng && src.MaxLng == dest.MaxLng;
    }
}

public class Boundry
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid BoundryId { get; set; }

    public Guid LocationId { get; set; }

    public virtual Location Location { get; set; }

    public decimal MinLat { get; set; }

    public decimal MaxLat { get; set; }

    public decimal MinLng { get; set; }

    public decimal MaxLng { get; set; }

    public bool Equals(Boundry boundry)
    {
        return MinLat == boundry.MinLat && MaxLat == boundry.MaxLat && MinLng == boundry.MinLng && MaxLng == boundry.MaxLng;
    }
}