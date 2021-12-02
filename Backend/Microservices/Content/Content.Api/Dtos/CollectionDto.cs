using System.ComponentModel.DataAnnotations;

namespace Content.Api.Dtos;

public class CollectionDto
{
    public Guid CollectionId { get; set; }

    [Required, MinLength(4), MaxLength(100)]
    public string Name { get; set; }

    public string Username { get; set; }

    public IEnumerable<PictureDto> Pictures { get; set; }
}