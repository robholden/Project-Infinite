using System.ComponentModel.DataAnnotations;

namespace Content.Api.Dtos;

public class TagDto
{
    [Required, MaxLength(255)]
    public string Value { get; set; }
}

public class TagAdminDto : TagDto
{
    public int Id { get; set; }

    public int Weight { get; set; }
}