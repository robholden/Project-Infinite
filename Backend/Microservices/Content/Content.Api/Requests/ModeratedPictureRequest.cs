using System.ComponentModel.DataAnnotations;

namespace Content.Api.Requests;

public class ModeratedPictureRequest
{
    [MaxLength(1000)]
    public string Notes { get; set; }

    public bool Outcome { get; set; }
}