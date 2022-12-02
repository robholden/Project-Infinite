
using AutoMapper;

using Comms.Core.Queries;
using Comms.Core.Services;
using Comms.Domain;
using Comms.Domain.Dtos;

using Library.Service.Api;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Comms.Api.Controllers;

public class UserController : BaseController<UserController>
{
    private readonly IUserSettingService _service;
    private readonly IUserSettingQueries _queries;

    public UserController(ILogger<UserController> logger, IMapper mapper, IUserSettingService service, IUserSettingQueries queries) : base(logger, mapper)
    {
        _service = service;
        _queries = queries;
    }

    [Authorize]
    [HttpGet]
    public async Task<UserSettingDto> Get()
    {
        var settings = await _queries.Get(LoggedInUser.Id);
        return _mapper.Map<UserSettingDto>(settings);
    }

    [Authorize]
    [HttpPut]
    public Task Update([FromBody] UserSettingDto settings)
    {
        // Validate model
        ThrowWhenStateIsInvalid();

        return _service.Update(LoggedInUser.Id, _mapper.Map<UserSetting>(settings));
    }

    [HttpPut("unsubscribe/{key}")]
    public Task Unsubscribe([FromRoute] Guid key)
    {
        return _service.Unsubscribe(key);
    }
}