
using AutoMapper;

using Content.Api.Dtos;
using Content.Api.Requests;
using Content.Api.Results;
using Content.Core.Queries;
using Content.Core.Services;
using Content.Domain;

using Library.Core;
using Library.Service.Api;
using Library.Service.Api.Auth;
using Library.Service.PubSub;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Content.Api.Controllers;

public class PictureController : BaseController<PictureController>
{
    private readonly IPictureService _service;
    private readonly IPictureQueries _pictureQueries;
    private readonly IPictureModerationQueries _moderationQueries;

    private readonly IReportPubSub _reportEvents;

    public PictureController(
        ILogger<PictureController> logger,
        IMapper mapper,
        IPictureService service,
        IPictureQueries pictureQueries,
        IPictureModerationQueries moderationQueries,
        IReportPubSub reportEvents
    ) : base(logger, mapper)
    {
        _service = service;
        _pictureQueries = pictureQueries;
        _moderationQueries = moderationQueries;

        _reportEvents = reportEvents;
    }

    [HttpGet]
    public async Task<PagedList<PictureDto>> Search(
        [FromQuery] string username = null,
        [FromQuery] bool likes = false,
        [FromQuery] bool drafts = false,
        [FromQuery] string collection = null,
        [FromQuery] string orderBy = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string orderDir = null
    )
    {
        // Build lookup options
        var options = new PictureQueryOptions()
        {
            Username = username ?? string.Empty,
            ShowLikes = likes,
            OrderBy = orderBy.ToEnum(PictureQueryOptions.OrderByEnum.None)
        };

        // Parse collection to guid
        if (Guid.TryParse(collection, out var collectionGuid))
        {
            options.CollectionId = collectionGuid;
        }

        // Only show drafts for the logged in user
        var author = string.Equals(LoggedInUser?.Username, options.Username, StringComparison.OrdinalIgnoreCase);
        if (author)
        {
            options.Draft = drafts;
        }

        // Create lookup request
        var request = new PagedListRequest<PictureQueryOptions>(page, pageSize, orderDir.ToEnum(OrderByDirection.Desc), options);
        return ToPagedPictures(await _pictureQueries.Lookup(request));
    }

    [HttpGet("explore")]
    public async Task<PagedList<PictureDto>> Explore([FromQuery] ExplorePicturesQuery query)
    {
        // If no filters are set, use featured pictures
        if (!query.HasFilter())
        {
            var featured = await _pictureQueries.Featured(query.Page, query.PageSize);
            return featured.ToDto<PictureDto>(_mapper);
        }

        // Build lookup options
        var options = new PictureQueryOptions()
        {
            Locations = query.Locations.UrlDecodeToArray(),
            Countries = query.Countries.UrlDecodeToArray(),
            Distance = query.Distance,
            Tags = query.Tags.UrlDecodeToArray(),
            Status = PictureStatus.Published,
            OrderBy = query.OrderBy.ToEnum(PictureQueryOptions.OrderByEnum.None)
        };

        // Create lookup request
        var request = new PagedListRequest<PictureQueryOptions>(query.Page, query.PageSize, query.OrderDir.ToEnum(OrderByDirection.Desc), options);
        var pictures = await _pictureQueries.Lookup(request);

        return ToPagedPictures(pictures);
    }

    [HttpGet("{id}")]
    public async Task<PictureDto> Get([FromRoute] Guid id)
    {
        var picture = DetermineWhichDto(await GetAndVerifyPicture(id));
        if (LoggedIn)
        {
            picture.Liked = await _pictureQueries.Likes(id, LoggedInUser.Id);
        }

        return picture;
    }

    [HttpGet("{id}/nearby")]
    public async Task<IEnumerable<PictureDto>> GetNearby([FromRoute] Guid id)
    {
        await GetAndVerifyPicture(id);
        var nearby = await _pictureQueries.NearBy(id);

        return _mapper.Map<IEnumerable<PictureDto>>(nearby);
    }

    [Authorize(Roles = nameof(UserLevel.Moderator))]
    [HttpGet("{id}/matches")]
    public async Task<IEnumerable<PictureDto>> GetMatches([FromRoute] Guid id)
    {
        var pictures = await _pictureQueries.Matches(id);
        return _mapper.Map<IEnumerable<PictureDto>>(pictures);
    }

    [Authorize]
    [HttpGet("drafts")]
    public async Task<PagedList<PictureDto>> SearchDrafts([FromQuery] string orderBy = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string orderDir = null)
    {
        // Build lookup options
        var options = new PictureQueryOptions()
        {
            Username = LoggedInUser.Username,
            Draft = true,
            OrderBy = orderBy.ToEnum(PictureQueryOptions.OrderByEnum.None)
        };

        // Create lookup request
        var request = new PagedListRequest<PictureQueryOptions>(page, pageSize, orderDir.ToEnum(OrderByDirection.Desc), options);
        var drafts = await _pictureQueries.Lookup(request);

        return ToPagedPictures(drafts);
    }

    [HttpPut("{id}/like")]
    public Task Like([FromRoute] Guid id, [FromBody] LikePictureRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        return _service.Like(id, LoggedInUser.ToRecord(), request.Liked);
    }

    [HttpPost("{id}/submit")]
    public async Task<PictureUserDto> Submit([FromRoute] Guid id)
    {
        // Verify user
        if (!await _pictureQueries.BelongsToUser(LoggedInUser.Id, id))
        {
            ThrowNotFound();
        }

        // Mark as pending
        await _service.Submit(id);

        return _mapper.Map<PictureUserDto>(await _pictureQueries.Get(id, LoggedInUser.Id));
    }

    [HttpPut("{id}")]
    public async Task<PictureUserDto> Update([FromRoute] Guid id, [FromBody] UpdatePictureRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Verify user
        if (!await _pictureQueries.BelongsToUser(LoggedInUser.Id, id))
        {
            ThrowNotFound();
        }

        // Update picture properties
        await _service.Update(id, request.Name, request.Tags, request.ConcealCoords, LoggedInUser.Level, request.Seed);

        return _mapper.Map<PictureUserDto>(await _pictureQueries.Get(id, LoggedInUser.Id));
    }

    [Authorize]
    [HttpGet("upload/verify")]
    public Task VerifyUpload([FromQuery] int uploads = 1) => _service.VerifyUserCanUpload(LoggedInUser.Id, uploads);

    [LimitRequest(maxLimit: 12, timeFrame: 2)]
    [Authorize]
    [HttpPost]
    public async Task<UploadPictureResult> Upload(IList<IFormFile> files)
    {
        // Ensure there's an upload
        if (files.Count == 0 || files[0].Length == 0)
        {
            Throw(ErrorCode.PictureRequired);
        }

        // Save image
        var (pictureId, errors) = await _service.Upload(LoggedInUser.ToRecord(), files[0], HttpContext.Connection.RemoteIpAddress.ToString());

        return new UploadPictureResult
        {
            Picture = pictureId.HasValue ? _mapper.Map<PictureUserDto>(await _pictureQueries.Get(pictureId.Value, LoggedInUser.Id)) : null,
            Errors = errors
        };
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task Delete([FromRoute] Guid id)
    {
        var picture = await GetAndVerifyPicture(id, true);
        await _service.Delete(picture.PictureId);
    }

    [Authorize]
    [HttpPost("{id}/report")]
    public async Task Report([FromRoute] Guid id, [FromBody] ReportPictureRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Verify access to picture
        var picture = await GetAndVerifyPicture(id);

        // Make sure not to report themselves
        if (picture.UserId == LoggedInUser.Id)
        {
            return;
        }

        // Invoke report event
        await _reportEvents.ReportPicture(new(LoggedInUser.ToRecord().ToUserRecord(), request.Reason, picture.PictureId, picture.ToUserRecord(), picture.Name, picture.Path));
    }

    [Authorize(Roles = nameof(UserLevel.Moderator))]
    [HttpGet("mod/next")]
    public async Task<Guid?> ModeratorNext()
    {
        var next = await _service.NextModeration(LoggedInUser.Username);
        return next?.PictureId;
    }

    [Authorize(Roles = nameof(UserLevel.Moderator))]
    [HttpGet("mod/search")]
    public async Task<PagedList<PictureModerationDto>> ModeratorSearch(
        [FromQuery] string username,
        [FromQuery] string orderBy = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string orderDir = null
    )
    {
        // Build lookup options
        var options = new PictureModerationQueryOptions()
        {
            Username = username,
            OrderBy = orderBy.ToEnum(PictureModerationQueryOptions.OrderByEnum.None),
        };

        // Create lookup request
        var request = new PagedListRequest<PictureModerationQueryOptions>(page, pageSize, orderDir.ToEnum(OrderByDirection.Asc), options);
        var moderations = await _moderationQueries.Lookup(LoggedInUser.Username, request);

        return moderations.ToDto<PictureModerationDto>(_mapper);
    }

    [Authorize(Roles = nameof(UserLevel.Moderator))]
    [HttpPost("mod/{id}")]
    public Task ModeratePicture([FromRoute] Guid id, [FromBody] ModeratedPictureRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        return _service.Moderate(id, LoggedInUser.ToRecord(), request.Outcome, request.Notes);
    }

    private async Task<Picture> GetAndVerifyPicture(Guid pictureId, bool enforceAuthor = false)
    {
        var picture = await _pictureQueries.Get(pictureId, LoggedInUser?.Id);
        if (picture == null)
        {
            ThrowNotFound();
        }

        if (picture.Status != PictureStatus.Published || enforceAuthor)
        {
            if (!User.Identity.IsAuthenticated)
            {
                ThrowNotFound();
            }

            var highAccess = IsAdmin || IsMod;
            if (picture.UserId != LoggedInUser.Id && !highAccess)
            {
                ThrowNotFound();
            }
        }

        return picture;
    }

    private PagedList<PictureDto> ToPagedPictures(PagedList<Picture> pagedList)
    {
        var pictures = pagedList.ToDto((rows) => rows.Select(r => DetermineWhichDto(r)));

        if (LoggedIn)
        {
            var likesMap = _pictureQueries.LikesMap(pagedList.Rows.Where(r => r.Status == PictureStatus.Published).Select(r => r.PictureId), LoggedInUser.Id);
            foreach (var p in pictures.Rows.Where(p => likesMap.ContainsKey(p.PictureId)))
            {
                p.Liked = likesMap[p.PictureId];
            }
        }

        return pictures;
    }

    private PictureDto DetermineWhichDto(Picture picture)
    {
        return LoggedIn && (picture.UserId == LoggedInUser.Id || IsMod) ? _mapper.Map<PictureUserDto>(picture) : _mapper.Map<PictureDto>(picture);
    }
}