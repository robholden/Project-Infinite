using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Library.Core;

namespace Content.Domain;

public class UserSetting : IUserId
{
    public UserSetting()
    {
    }

    public UserSetting(Guid userId)
    {
        UserId = userId;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid SettingId { get; set; }

    public Guid UserId { get; set; }

    public int MaxPictureSize { get; set; } = 10;

    public int MinPictureResolutionX { get; set; } = 400;

    public int MinPictureResolutionY { get; set; } = 300;

    public int UploadLimit { get; set; } = 10;

    public bool UploadEnabled { get; set; } = true;
}