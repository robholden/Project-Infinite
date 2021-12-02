using System.ComponentModel.DataAnnotations;

namespace Content.Domain;

public class Tag
{
    public Tag()
    {
    }

    public Tag(string value, int weight = 0)
    {
        Value = value;
        Weight = weight;
    }

    [Key]
    public int Id { get; set; }

    [Required, MaxLength(255)]
    public string Value { get; set; }

    public int Weight { get; set; }

    public virtual ICollection<Picture> Pictures { get; set; }
}