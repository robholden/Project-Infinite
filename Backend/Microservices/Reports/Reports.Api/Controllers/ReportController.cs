
using AutoMapper;

using Library.Core;
using Library.Service.Api;
using Library.Service.Api.Auth;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Report.Api.Requests;

using Reports.Core.Queries;
using Reports.Core.Services;
using Reports.Domain;
using Reports.Domain.Dtos;

namespace Reports.Api.Controllers;

[Authorize(Roles = nameof(UserLevel.Moderator))]
public class ReportController : BaseController<ReportController>
{
    private readonly IReportService _service;
    private readonly IReportQueries _queries;

    public ReportController(ILogger<ReportController> logger, IMapper mapper, IReportService service, IReportQueries queries) : base(logger, mapper)
    {
        _service = service;
        _queries = queries;
    }

    [HttpGet("users")]
    public async Task<PagedList<UserReportDto>> SearchUserReports([FromQuery] bool? actioned = false, [FromQuery] string orderBy = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string orderDir = null)
    {
        // Build lookup options
        var options = new ReportQueryOptions()
        {
            Actioned = actioned,
            OrderBy = orderBy.ToEnum(ReportQueryOptions.OrderByEnum.None)
        };

        // Create lookup request
        var request = new PagedListRequest<ReportQueryOptions>(page, pageSize, orderDir.ToEnum(OrderByDirection.Desc), options);
        var reports = await _queries.Lookup<UserReport, UserReportInstance>(request);

        return reports.ToDto<UserReportDto>(_mapper);
    }

    [HttpGet("pictures")]
    public async Task<PagedList<PictureReportDto>> SearchPictureReports([FromQuery] bool? actioned = false, [FromQuery] string orderBy = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string orderDir = null)
    {
        // Build lookup options
        var options = new ReportQueryOptions()
        {
            Actioned = actioned,
            OrderBy = orderBy.ToEnum(ReportQueryOptions.OrderByEnum.None)
        };

        // Create lookup request
        var request = new PagedListRequest<ReportQueryOptions>(page, pageSize, orderDir.ToEnum(OrderByDirection.Desc), options);
        var reports = await _queries.Lookup<PictureReport, PictureReportInstance>(request);

        return reports.ToDto<PictureReportDto>(_mapper);
    }

    [HttpPost("user/{reportId}/action")]
    public async Task ActionUserReport([FromRoute] Guid reportId, [FromBody] ReportActionedRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Publish report event
        await _service.ActionUserReport(reportId, request.Action, LoggedInUser.ToRecord(), request.Notes, request.SendEmail);
    }

    [HttpPost("picture/{reportId}/action")]
    public async Task ActionPictureReport([FromRoute] Guid reportId, [FromBody] ReportActionedRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Publish report event
        await _service.ActionPictureReport(reportId, request.Action, LoggedInUser.ToRecord(), request.Notes, request.SendEmail);
    }
}