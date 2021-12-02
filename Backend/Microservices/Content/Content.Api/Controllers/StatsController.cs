
using AutoMapper;

using Content.Api.Results;
using Content.Domain;

using Library.Service.Api;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Content.Api.Controllers;

public class StatsController : BaseController<StatsController>
{
    private readonly ContentContext _ctx;

    public StatsController(ILogger<StatsController> logger, IMapper mapper, ContentContext ctx) : base(logger, mapper)
    {
        _ctx = ctx;
    }

    [HttpGet("{username}")]
    public async Task<UserStatsResult> GetByUsername([FromRoute] string username)
    {
        username = username.ToLower();
        var drafts = 0;
        var pending = 0;

        // TODO: Does this return a user?
        if (LoggedIn && username == LoggedInUser.Username.ToLower())
        {
            var userId = LoggedInUser.Id;
            drafts = await _ctx.Pictures.CountAsync(x => x.UserId == userId && x.Status == PictureStatus.Draft);
            pending = await _ctx.Pictures.CountAsync(x => x.UserId == userId && x.Status == PictureStatus.PendingApproval);
        }

        var published = await _ctx.Pictures.CountAsync(x => x.Username == username && x.Status == PictureStatus.Published);
        var collections = await _ctx.Collections.CountAsync(x => x.Username.ToLower() == username);
        var likes = await _ctx.PictureLikes.CountAsync(x => x.Username.ToLower() == username);

        return new UserStatsResult(drafts, pending, published, collections, likes);
    }
}