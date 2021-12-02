namespace Reports.Api.Dtos;

public class ReportDto
{
    public Guid ReportId { get; set; }

    public string Username { get; set; }

    public ReportActionDto Action { get; set; }

    public DateTime Date { get; set; }

    public IEnumerable<ReportStats> Stats { get; set; }

    public IEnumerable<ReportInstanceDto> Reports { get; set; }
}

public class ReportInstanceDto
{
    public string Username { get; set; }

    public int Reason { get; set; }

    public DateTime Date { get; set; }
}