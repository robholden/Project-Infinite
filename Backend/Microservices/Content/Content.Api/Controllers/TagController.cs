
using AutoMapper;

using Content.Api.Dtos;
using Content.Api.Requests;
using Content.Core.Queries;
using Content.Core.Services;
using Content.Domain;

using Library.Core;
using Library.Service.Api;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Content.Api.Controllers;

public class TagController : BaseController<TagController>
{
    private readonly ITagService _service;
    private readonly ITagQueries _queries;

    public TagController(
        ILogger<TagController> logger,
        IMapper mapper,
        ITagService service,
        ITagQueries queries
    ) : base(logger, mapper)
    {
        _service = service;
        _queries = queries;
    }

    [HttpGet]
    public IEnumerable<TagDto> GetAll()
    {
        var tags = _queries.GetAll();
        return IsAdmin ? _mapper.Map<IEnumerable<TagAdminDto>>(tags) : _mapper.Map<IEnumerable<TagDto>>(tags);
    }

    [Authorize(Roles = nameof(UserLevel.Admin))]
    [HttpPost]
    public async Task Save([FromBody] SaveTagRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        if (request.ToAdd?.Any() == true) await _service.Create(_mapper.Map<IEnumerable<Tag>>(request.ToAdd));
        if (request.ToUpdate?.Any() == true) await _service.Update(_mapper.Map<IEnumerable<Tag>>(request.ToUpdate));
        if (request.ToDelete?.Any() == true) await _service.Delete(request.ToDelete);
    }

    [Authorize(Roles = nameof(UserLevel.Admin))]
    [HttpDelete("{id}")]
    public Task Delete([FromRoute] int id) => _service.Delete(id);
}