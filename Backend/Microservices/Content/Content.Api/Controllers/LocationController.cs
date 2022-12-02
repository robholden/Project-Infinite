
using AutoMapper;

using Content.Api.Requests;
using Content.Core.Queries;
using Content.Core.Services;
using Content.Domain;
using Content.Domain.Dtos;

using Library.Core;
using Library.Service.Api;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Content.Api.Controllers;

public class LocationController : BaseController<LocationController>
{
    private readonly ILocationService _service;
    private readonly ILocationQueries _queries;

    public LocationController(
        ILogger<LocationController> logger,
        IMapper mapper,
        ILocationService service,
        ILocationQueries queries
    ) : base(logger, mapper)
    {
        _service = service;
        _queries = queries;
    }

    [Authorize(Roles = nameof(UserLevel.Admin))]
    [HttpGet]
    public async Task<PagedList<LocationDto>> Get(
        [FromQuery] string name,
        [FromQuery] string orderBy = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string orderDir = null
    )
    {
        // Build lookup options
        var options = new LocationQueryOptions()
        {
            Name = name,
            OrderBy = orderBy.ToEnum(LocationQueryOptions.OrderByEnum.None)
        };

        // Create lookup request
        var request = new PagedListRequest<LocationQueryOptions>(page, pageSize, orderDir.ToEnum(OrderByDirection.Desc), options);
        var pictures = await _queries.Lookup(request);

        return pictures.ToDto<LocationDto>(_mapper);
    }

    [Authorize(Roles = nameof(UserLevel.Admin))]
    [HttpPut("{id}/name")]
    public async Task UpdateName([FromRoute] Guid id, [FromBody] UpdateNameRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        await _service.UpdateName(id, request.Name);
    }

    [Authorize(Roles = nameof(UserLevel.Admin))]
    [HttpPut("{id}/code")]
    public async Task UpdateCode([FromRoute] Guid id, [FromBody] UpdateNameRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        await _service.UpdateCode(id, request.Name);
    }

    [Authorize(Roles = nameof(UserLevel.Admin))]
    [HttpPut("{id}/boundry")]
    public async Task UpdateBoundry([FromRoute] Guid id, [FromBody] BoundryDto boundry)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        await _service.UpdateBoundry(id, _mapper.Map<Boundry>(boundry));
    }
}