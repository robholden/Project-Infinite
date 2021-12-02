namespace Content.Api.Dtos;

public class PictureModerationDto
{
    public Guid PictureId { get; set; }

    public PictureUserDto Picture { get; set; }

    public DateTime Date { get; set; }
}