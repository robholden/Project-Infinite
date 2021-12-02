
using AutoMapper;

using Comms.Api.Dtos;
using Comms.Core.Queries;
using Comms.Core.Services;

using Library.Core;
using Library.Core.Enums;
using Library.Service.Api;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Content.Api.Controllers;

[Authorize(Roles = nameof(UserLevel.Moderator))]
[Route("notification/mod")]
public class NotificationModController : BaseController<NotificationModController>
{
    private readonly INotificationService _service;
    private readonly INotificationQueries _queries;

    public NotificationModController(
        ILogger<NotificationModController> logger,
        IMapper mapper,
        INotificationService service,
        INotificationQueries queries
    ) : base(logger, mapper)
    {
        _service = service;
        _queries = queries;
    }


    [HttpGet]
    public async Task<PagedList<NotificationDto>> Get([FromQuery] int page = 1)
    {
        // Build lookup options
        var options = new NotificationQueryOptions()
        {
            UserId = LoggedInUser.Id,
            OrderBy = NotificationQueryOptions.OrderByEnum.Date,
            UserLevels = new() { UserLevel.Moderator }
        };

        // Create lookup request
        var request = new PagedListRequest<NotificationQueryOptions>(page, 10, OrderByDirection.Desc, options);
        var notifs = await _queries.Lookup(request);

        // Mark as viewed
        await _service.Viewed(LoggedInUser.Id, UserLevel.Moderator);

        return notifs.ToDto<NotificationDto>(_mapper);
    }

    [HttpGet("count/unread")]
    public Task<int> CountUnread()
    {
        // Build lookup options
        var options = new NotificationQueryOptions()
        {
            UserId = LoggedInUser.Id,
            UserLevels = new() { UserLevel.Moderator },
            Read = false
        };

        // Create lookup request
        return _queries.Count(options);
    }

    [HttpGet("count/latest")]
    public Task<int> CountLatest()
    {
        // Build lookup options
        var options = new NotificationQueryOptions()
        {
            UserId = LoggedInUser.Id,
            UserLevels = new() { UserLevel.Moderator },
            Viewed = false
        };

        // Create lookup request
        return _queries.Count(options);
    }

    [HttpPut]
    public Task MarkAllAsRead()
    {
        return _service.MarkAllAsRead(LoggedInUser.Id, UserLevel.Moderator);
    }

    [HttpPut("{id}")]
    public async Task MarkAsRead([FromRoute] Guid id)
    {
        if (!await _queries.ExistsForUser(LoggedInUser.Id, id))
        {
            ThrowNotFound();
        }

        await _service.MarkAsRead(id);
    }
}