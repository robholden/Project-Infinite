namespace Comms.Domain.Dtos;

public class UserSettingDto
{
    public bool MarketingEmail { get; set; }

    public bool PictureApprovedEmail { get; set; }

    public bool PictureUnapprovedEmail { get; set; }

    public bool PictureLikedEmail { get; set; }
}