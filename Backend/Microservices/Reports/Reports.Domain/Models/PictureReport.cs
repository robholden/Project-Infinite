using System.ComponentModel.DataAnnotations;

using Library.Core.Enums;
using Library.Core.Models;

namespace Reports.Domain;

public class PictureReport : Report<PictureReportInstance>, IUser
{
    public PictureReport()
    {
    }

    public PictureReport(IUser pictureUser, Guid pictureId, string pictureName, string picturePath) : base(pictureUser)
    {
        PictureId = pictureId;
        PictureName = pictureName;
        PicturePath = picturePath;
    }

    [Required]
    public Guid PictureId { get; set; }

    [Required, MaxLength(100)]
    public string PictureName { get; set; }

    [Required, MaxLength(255)]
    public string PicturePath { get; set; }
}

public class PictureReportInstance : ReportInstance
{
    public PictureReportInstance()
    {
    }

    public PictureReportInstance(IUser user, ReportPictureReason reason) : base(user)
    {
        Reason = reason;
    }

    public ReportPictureReason Reason { get; set; }

    public Guid ReportId { get; set; }

    public virtual PictureReport PictureReport { get; set; }
}