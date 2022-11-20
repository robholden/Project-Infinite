namespace Content.Api.Dtos;

public class UserSettingDto
{
    public int MaxPictureSize { get; set; }

    public int MinPictureResolutionX { get; set; }

    public int MinPictureResolutionY { get; set; }

    public int DraftLimit { get; set; }

    public int DailyUploadLimit { get; set; }

    public bool UploadEnabled { get; set; }
}