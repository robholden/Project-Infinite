using AutoMapper;

using Content.Core.Queries;
using Content.Core.Services;
using Content.Domain.Dtos;

using Library.Core;
using Library.Service.Api;
using Library.Service.Api.Requests;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Content.Api.Controllers;

public class PostController : BaseController<PostController>
{
    private readonly IMemoryCache _cache;

    private readonly IPostQueries _postQueries;
    private readonly IPostService _postService;

    public PostController(ILogger<PostController> logger, IMapper mapper, IMemoryCache cache, IPostQueries postQueries, IPostService postService)
        : base(logger, mapper)
    {
        _cache = cache;
        _postQueries = postQueries;
        _postService = postService;
    }

    [HttpGet("{id}")]
    public async Task<PostDto> Create([FromRoute] Guid id)
    {
        var post = await _postQueries.Get(id);
        if (post == null) ThrowNotFound();

        return _mapper.Map<PostDto>(post);
    }

    [HttpGet]
    public async Task<PagedList<PostDto>> Lookup([FromQuery] LookupQueryRequest request)
    {
        // Check cache
        if (_cache.TryGetValue<PagedList<PostDto>>(request.CacheKey, out var cached))
        {
            return cached;
        }

        var options = new PostQueryOptions
        {
            OrderBy = PostQueryOptions.OrderByEnum.CreatedDate,
            FilterBy = request.filterBy,
            FilterText = request.filter,
            Username = request.username
        };
        var posts = await _postQueries.Lookup(new PagedListRequest<PostQueryOptions>(request.page, request.pageSize, request.orderDir.ToEnum(OrderByDirection.Desc), options));
        var paged = posts.ToDto<PostDto>(_mapper);

        // Cache results
        _cache.Set(request.CacheKey, paged, TimeSpan.FromSeconds(15));

        return paged;
    }

    [Authorize]
    [HttpPost]
    public async Task<PostDto> Create([FromBody] PostDto request)
    {
        var post = await _postService.Create(request, LoggedInUser.AsRecord());
        return _mapper.Map<PostDto>(post);
    }

    [Authorize]
    [HttpPost("{id}")]
    public async Task<PostDto> Update([FromRoute] Guid id, [FromBody] PostDto request)
    {
        // Ensure this is their post to update
        if (!IsMod && !(await _postQueries.DoesBelongToUser(id, LoggedInUser.Id)))
        {
            ThrowForbidden();
        }

        var post = await _postService.Update(id, request);
        return _mapper.Map<PostDto>(post);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task Delete([FromRoute] Guid id)
    {
        // Ensure this is their post to delete
        if (!IsMod && !(await _postQueries.DoesBelongToUser(id, LoggedInUser.Id)))
        {
            ThrowForbidden();
        }

        await _postService.Delete(id);
    }

    public class LookupQueryRequest : PagedListQuery
    {
        [FromQuery]
        public string filter { get; set; }

        [FromQuery]
        public PostQueryOptions.FilterParams? filterBy { get; set; }

        [FromQuery]
        public string username { get; set; }

        public string CacheKey => $"posts_lookup_{username}{filter}{filterBy}{orderDir}{page}{pageSize}";
    }
}