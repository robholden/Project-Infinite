namespace Reports.Api.Dtos;

public class ReportStats
{
    public ReportStats()
    {
    }

    public ReportStats(int reason, int total)
    {
        Reason = reason;
        Total = total;
    }

    public int Total { get; set; }

    public int Reason { get; set; }
}