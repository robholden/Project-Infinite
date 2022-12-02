
using AutoMapper;

using Content.Core.Queries;
using Content.Core.Services;
using Content.Domain.Dtos;

using Library.Core;
using Library.Service.Api;
using Library.Service.Api.Auth;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Content.Api.Controllers;

public class CollectionController : BaseController<CollectionController>
{
    private readonly ICollectionService _service;
    private readonly ICollectionQueries _queries;

    public CollectionController(
        ILogger<CollectionController> logger,
        IMapper mapper,
        ICollectionService service,
        ICollectionQueries queries
    ) : base(logger, mapper)
    {
        _service = service;
        _queries = queries;
    }

    [HttpGet]
    public async Task<PagedList<CollectionDto>> Search(
        [FromQuery] string username,
        [FromQuery] string orderBy = null,
        [FromQuery] Guid? includePictureId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string orderDir = null
    )
    {
        // Build lookup options
        var options = new CollectionQueryOptions()
        {
            Username = username,
            IncludePictureId = includePictureId,
            OrderBy = orderBy.ToEnum(CollectionQueryOptions.OrderByEnum.None)
        };

        // Create lookup request
        var request = new PagedListRequest<CollectionQueryOptions>(page, pageSize, orderDir.ToEnum(OrderByDirection.Desc), options);
        var results = await _queries.Lookup(request);

        return results.ToDto<CollectionDto>(_mapper);
    }

    [HttpGet("{id}")]
    public async Task<CollectionDto> Get([FromRoute] Guid id)
    {
        var collection = await _queries.Get(id);
        if (collection == null)
        {
            ThrowNotFound();
        }

        return _mapper.Map<CollectionDto>(collection);
    }

    [Authorize]
    [HttpPost]
    public async Task<CollectionDto> Create([FromBody] CollectionDto request)
    {
        ThrowWhenStateIsInvalid();

        var collectionId = await _service.Create(LoggedInUser.ToRecord(), request.Name);
        return _mapper.Map<CollectionDto>(await _queries.Get(collectionId));
    }

    [Authorize]
    [HttpPut("{collectionId}/{pictureId}")]
    public async Task AddOrRemove([FromRoute] Guid collectionId, [FromRoute] Guid pictureId)
    {
        // Ensure the collection is theirs
        if (!await _queries.ExistsForUser(LoggedInUser.Id, collectionId))
        {
            ThrowNotFound();
        }

        await _service.AddOrRemovePicture(collectionId, pictureId);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<CollectionDto> Update([FromRoute] Guid id, [FromBody] CollectionDto request)
    {
        ThrowWhenStateIsInvalid();

        var exists = await _queries.ExistsForUser(LoggedInUser.Id, id);
        if (!exists)
        {
            ThrowNotFound();
        }

        await _service.Update(id, request.Name);

        return _mapper.Map<CollectionDto>(await _queries.Get(id));
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task Delete([FromRoute] Guid id)
    {
        var exists = await _queries.ExistsForUser(LoggedInUser.Id, id);
        if (!exists)
        {
            ThrowNotFound();
        }

        await _service.Delete(id);
    }
}