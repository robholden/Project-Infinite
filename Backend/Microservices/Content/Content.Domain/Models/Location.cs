using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Content.Domain;

public class Location : Coords
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid LocationId { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Code { get; set; }

    public virtual Country Country { get; set; }

    public virtual Boundry Boundry { get; set; }

    public virtual ICollection<Picture> Pictures { get; set; }
}