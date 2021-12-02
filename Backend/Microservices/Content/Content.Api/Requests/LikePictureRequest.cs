using System.ComponentModel.DataAnnotations;

namespace Content.Api.Requests;

public class LikePictureRequest
{
    [Required]
    public bool Liked { get; set; }
}