using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Content.Domain;

public class Country
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid CountryId { get; set; }

    [Required, MaxLength(10)]
    public string Code { get; set; }

    [Required]
    public decimal Lat { get; set; } = 0;

    [Required]
    public decimal Lng { get; set; } = 0;

    [Required, MaxLength(255)]
    public string Name { get; set; }
}