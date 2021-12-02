namespace Reports.Api.Dtos;

public class PictureReportDto : ReportDto
{
    public Guid PictureId { get; set; }

    public string PictureName { get; set; }

    public string PicturePath { get; set; }
}