
using AutoMapper;

using Content.Api.Dtos;
using Content.Core.Queries;
using Content.Core.Services;
using Content.Domain;

using Library.Core.Enums;
using Library.Service.Api;
using Library.Service.PubSub;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Content.Api.Controllers;

[Authorize]
public class UserController : BaseController<UserController>
{
    private readonly IUserSettingService _service;
    private readonly IUserSettingQueries _queries;

    private readonly ISocketsPubSub _socketEvents;

    public UserController(ILogger<UserController> logger, IMapper mapper, IUserSettingService service, IUserSettingQueries queries, ISocketsPubSub socketsEvents)
        : base(logger, mapper)
    {
        _service = service;
        _queries = queries;

        _socketEvents = socketsEvents;
    }

    [HttpGet]
    public async Task<UserSettingDto> Get()
    {
        var setting = await _queries.Get(LoggedInUser.Id) ?? new();
        return _mapper.Map<UserSettingDto>(setting);
    }

    [Authorize(Roles = nameof(UserLevel.Admin))]
    [HttpGet("{userId}")]
    public async Task<UserSettingDto> Get([FromRoute] Guid userId)
    {
        var setting = await _queries.Get(LoggedInUser.Id) ?? new();
        return _mapper.Map<UserSettingDto>(setting);
    }

    [Authorize(Roles = nameof(UserLevel.Admin))]
    [HttpPut("{userId}")]
    public async Task Update([FromRoute] Guid userId, [FromBody] UserSettingDto userData)
    {
        // Validate model
        ThrowWhenStateIsInvalid();

        // Update db
        var user = await _service.AddOrUpdate(userId, _mapper.Map<UserSetting>(userData));

        // Send socket update to UI
        if (user != null)
        {
            _ = _socketEvents.UpdatedUserSettings(new(user.UserId, _mapper.Map<UserSettingDto>(user)));
        }
    }
}