using System.ComponentModel.DataAnnotations;

using Library.Core;

namespace Comms.Domain;

public class UserSetting : IUser
{
    public UserSetting()
    {
    }

    public UserSetting(Guid userId)
    {
        UserId = userId;
    }

    [Key]
    public Guid UserId { get; set; }

    [MinLength(3), MaxLength(50)]
    public string Username { get; set; }

    [MaxLength(100)]
    public string Name { get; set; }

    [EmailAddress, MaxLength(255)]
    public string Email { get; set; }

    public Guid? MarketingOptOutKey { get; set; }

    public bool MarketingEmail { get; set; }

    public bool PictureApprovedEmail { get; set; }

    public bool PictureUnapprovedEmail { get; set; }

    public bool PictureLikedEmail { get; set; }

    public virtual ICollection<NotificationEntry> NotificationEntries { get; set; }
}