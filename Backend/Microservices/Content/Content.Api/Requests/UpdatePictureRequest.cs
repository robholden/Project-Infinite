using System.ComponentModel.DataAnnotations;

namespace Content.Api.Requests;

public class UpdatePictureRequest
{
    [Required, MinLength(4), MaxLength(100)]
    public string Name { get; set; }

    [Required]
    public bool ConcealCoords { get; set; }

    [Required]
    public IList<string> Tags { get; set; }

    [Required]
    public string Seed { get; set; }
}