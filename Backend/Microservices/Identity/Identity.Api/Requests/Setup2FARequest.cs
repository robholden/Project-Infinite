using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Requests;

public class Setup2FARequest
{
    [Required]
    public string Type { get; set; }

    [MaxLength(50), DataType(DataType.PhoneNumber)]
    public string Mobile { get; set; }
}