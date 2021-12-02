namespace Content.Api.Dtos;

public class UserSettingDto
{
    public int MaxPictureSize { get; set; }

    public int MinPictureResolutionX { get; set; }

    public int MinPictureResolutionY { get; set; }

    public int UploadLimit { get; set; }

    public bool UploadEnabled { get; set; }
}