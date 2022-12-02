
using AutoMapper;

using Comms.Core.Queries;
using Comms.Core.Services;
using Comms.Domain.Dtos;

using Library.Core;
using Library.Service.Api;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Content.Api.Controllers;

[Authorize]
public class NotificationController : BaseController<NotificationController>
{
    private readonly INotificationService _service;
    private readonly INotificationQueries _queries;

    public NotificationController(
        ILogger<NotificationController> logger,
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
            OrderBy = NotificationQueryOptions.OrderByEnum.Date
        };

        // Create lookup request
        var request = new PagedListRequest<NotificationQueryOptions>(page, 10, OrderByDirection.Desc, options);
        var notifs = await _queries.Lookup(request);

        // Mark as viewed
        await _service.Viewed(LoggedInUser.Id);

        return notifs.ToDto<NotificationDto>(_mapper);
    }

    [HttpGet("count/unread")]
    public Task<int> CountUnread()
    {
        // Build lookup options
        var options = new NotificationQueryOptions() { UserId = LoggedInUser.Id, Read = false };

        // Create lookup request
        return _queries.Count(options);
    }

    [HttpGet("count/latest")]
    public Task<int> CountLatest()
    {
        // Build lookup options
        var options = new NotificationQueryOptions() { UserId = LoggedInUser.Id, Viewed = false };

        // Create lookup request
        return _queries.Count(options);
    }

    [HttpPut]
    public Task MarkAllAsRead()
    {
        return _service.MarkAllAsRead(LoggedInUser.Id);
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

    [HttpDelete("{id}")]
    public async Task Delete([FromRoute] Guid id)
    {
        if (!await _queries.ExistsForUser(LoggedInUser.Id, id))
        {
            ThrowNotFound();
        }

        await _service.Delete(id);
    }
}