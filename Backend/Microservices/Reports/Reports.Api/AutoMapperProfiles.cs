
using AutoMapper;

using Reports.Domain;
using Reports.Domain.Dtos;

namespace Reports.Api;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<ReportActionDto, ReportAction>().ReverseMap();
        CreateMap<UserReportDto, UserReport>().ReverseMap().ForMember(dest => dest.Stats, opt => opt.MapFrom(src => GenerateUserStats(src)));
        CreateMap<PictureReportDto, PictureReport>().ReverseMap().ForMember(dest => dest.Stats, opt => opt.MapFrom(src => GeneratePictureStats(src)));
        CreateMap<ReportInstanceDto, UserReportInstance>().ReverseMap();
        CreateMap<ReportInstanceDto, PictureReportInstance>().ReverseMap();
    }

    private static IEnumerable<ReportStats> GenerateUserStats(UserReport report)
    {
        var reports = report.Reports ?? new List<UserReportInstance>();
        return reports.GroupBy(x => x.Reason, (reason, reports) => new ReportStats((int)reason, reports.Count())).OrderByDescending(x => x.Total);
    }

    private static IEnumerable<ReportStats> GeneratePictureStats(PictureReport report)
    {
        var reports = report.Reports ?? new List<PictureReportInstance>();
        return reports.GroupBy(x => x.Reason, (reason, reports) => new ReportStats((int)reason, reports.Count())).OrderByDescending(x => x.Total);
    }
}