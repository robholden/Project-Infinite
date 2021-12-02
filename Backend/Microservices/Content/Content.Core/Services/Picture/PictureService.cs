
using System.Globalization;

using Content.Core.Queries;
using Content.Domain;

using Library.Core;
using Library.Core.Enums;
using Library.Core.Models;
using Library.Service.PubSub;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;

namespace Content.Core.Services;

public class PictureService : IPictureService
{
    private readonly ContentContext _ctx;
    private readonly PictureSettings _settings;
    private readonly ILocationQueries _locationQueries;

    private readonly ICommsPubSub _commEvents;
    private readonly ISocketsPubSub _socketEvents;

    public PictureService(ContentContext ctx, IOptions<PictureSettings> options, ILocationQueries locationQueries, ICommsPubSub commEvents, ISocketsPubSub socketEvents)
    {
        _ctx = ctx;
        _settings = options.Value;
        _locationQueries = locationQueries;

        _commEvents = commEvents;
        _socketEvents = socketEvents;
    }

    public async Task<UserSetting> VerifyUserCanUpload(Guid userId, int uploadCount)
    {
        // Can they upload?
        var (settings, _) = await _ctx.UserSettings.FindWithDefaultAsync(x => x.UserId == userId, new());
        if (!settings.UploadEnabled)
        {
            throw new SiteException(ErrorCode.UploadNotAllowed);
        }

        // Set draft limit
        var limit = settings.UploadLimit;

        // A user can only have (n) drafts at one time
        var drafts = await _ctx.Pictures.CountAsync(p => p.UserId == userId && p.Status != PictureStatus.Published);
        if (drafts > limit)
        {
            throw new SiteException(ErrorCode.DraftLimitReached, limit);
        }

        // Are they uploading too many?
        if ((drafts + uploadCount) > limit)
        {
            throw new SiteException(ErrorCode.UploadWouldExceedDraftLimit, limit);
        }

        // Check this user's number of uploads for today
        var start = DateTime.Now.ResetToDay();
        var end = start.AddHours(24);
        var uploaded = await _ctx.Pictures.CountAsync(p => p.UserId == userId && p.CreatedDate >= start && p.CreatedDate <= end);

        if (uploaded >= settings.UploadLimit)
        {
            throw new SiteException(ErrorCode.UploadLimitReached);
        }

        return settings;
    }

    public async Task<(Guid? pictureId, IList<ErrorCodeDto> errors)> Upload(IUser user, string filePath, string ipAddress)
    {
        if (!File.Exists(filePath))
        {
            throw new SiteException(ErrorCode.PathNotFound);
        }

        var bytes = await File.ReadAllBytesAsync(filePath);
        return await Upload(user, bytes, Path.GetFileName(filePath), ipAddress);
    }

    public Task<(Guid? pictureId, IList<ErrorCodeDto> errors)> Upload(IUser user, IFormFile file, string ipAddress)
    {
        var reader = new BinaryReader(file.OpenReadStream());
        var bytes = reader.ReadBytes((int)file.Length);

        return Upload(user, bytes, file.FileName, ipAddress);
    }

    public async Task Update(Guid pictureId, string name, IEnumerable<string> tags, bool concealCoords, UserLevel userLevel, string seed = null)
    {
        // Get picture
        var picture = await _ctx.Pictures.Include(p => p.Tags).FindAsync(p => p.PictureId == pictureId);

        // Validate
        if (seed != null && !picture.IsSeedValid(seed))
        {
            throw new SiteException(ErrorCode.PictureModified);
        }

        // Only admins can update details once published
        if (userLevel == UserLevel.Admin || picture.Status == PictureStatus.Draft)
        {
            picture.Name = name;
            picture.Tags = await _ctx.Tags.Where(t => tags.Contains(t.Value)).Take(10).ToListAsync();
        }

        picture.ConcealCoords = concealCoords;
        picture.UpdatedDate = DateTime.UtcNow;

        await _ctx.Put(picture);
    }

    public async Task Submit(Guid pictureId)
    {
        var picture = await _ctx.Pictures.FindAsync(p => p.PictureId == pictureId);
        if (picture.Status == PictureStatus.PendingApproval)
        {
            return;
        }

        picture.Status = PictureStatus.PendingApproval;
        picture.UpdatedDate = DateTime.UtcNow;
        picture = await _ctx.Put(picture);

        // Add a moderation request for this picture
        await _ctx.Post(new PictureModeration(picture.PictureId));

        // Create approval notification for moderators
        _ = _commEvents?.AddGeneralNotification(new(UserLevel.Moderator, NotificationType.NewPictureApproval, new($"{ picture.PictureId }", picture.Name, picture.Path)));
    }

    public async Task<PictureModeration> NextModeration(string username)
    {
        // Get request from db
        var request = await _ctx.PictureModerations
              .Include(x => x.Picture.Tags)
              .Include(x => x.Picture).ThenInclude(p => p.Location).ThenInclude(l => l.Country)
              .OrderBy(x => x.Date)
              .FindOrNullAsync(x => x.LockedBy == username || x.LockedBy == null || (x.LockedAt.HasValue && x.LockedAt < DateTime.UtcNow.AddMinutes(-1)));

        request.LockedBy = username;
        request.LockedAt = DateTime.UtcNow;

        return await _ctx.Put(request);
    }

    public async Task Moderate(Guid pictureId, IUser moderatedBy, bool outcome, string notes)
    {
        // Get request from db
        var request = await _ctx.PictureModerations
            .Include(x => x.Picture)
            .FindAsync(x => x.PictureId == pictureId);

        // Store picture as it would be lost on deletion
        var picture = request.Picture;

        // Action the outcome of the picture
        if (!outcome)
        {
            // Physically remove picture from site
            // Which in turn, will remove this request
            await Delete(pictureId);
        }
        else
        {
            // Change the picture's status to published
            request.Picture.Status = PictureStatus.Published;
            await _ctx.Put(request.Picture);

            // Remove request
            await _ctx.Delete(request);
        }

        // Notify user of the result
        var type = outcome ? NotificationType.PictureApproved : NotificationType.PictureUnapproved;
        var contentKey = outcome ? $"{ picture.PictureId }" : picture.Name;
        var contentMessage = outcome ? picture.Name : notes;
        var contentImage = outcome ? picture.Path : "";
        var content = new NotificationContent(contentKey, contentMessage, contentImage);

        _ = Task.WhenAll(
            _commEvents?.AddNotification(new(picture.ToUserRecord(), type, content)),
            _commEvents?.UpdateGeneralNotification(new(UserLevel.Moderator, NotificationType.NewPictureApproval, $"{ picture.PictureId }", type, content)),
            _socketEvents?.ModeratedPicture(new(picture.PictureId, outcome))
        );
    }

    public async Task Like(Guid pictureId, IUser triggeredUser, bool liked)
    {
        // Verify the picture exists
        var picture = await _ctx.Pictures
            .Include(p => p.Likes.Where(l => l.UserId == triggeredUser.UserId))
            .FindOrNullAsync(p => p.PictureId == pictureId && p.Status == PictureStatus.Published);

        // Stop if the state is the same
        if ((liked && picture.Likes.Count > 0) || (!liked && picture.Likes.Count == 0))
        {
            return;
        }

        // Add or remove like from pictures
        if (liked) picture.Likes.Add(new(pictureId, triggeredUser));
        else picture.Likes.Remove(picture.Likes.First());

        await _ctx.SaveChangesAsync();

        // Add/remove notification
        if (liked) _commEvents?.AddNotification(new(picture.ToUserRecord(), NotificationType.NewLike, new(picture.PictureId.ToString(), picture.Name, picture.Path), triggeredUser.ToUserRecord()));
        else _commEvents?.RemoveNotification(new(picture.UserId, NotificationType.NewLike, picture.PictureId.ToString(), triggeredUser.UserId));
    }

    public async Task<Picture> Delete(Guid pictureId)
    {
        var picture = await _ctx.Pictures.FirstOrDefaultAsync(x => x.PictureId == pictureId);
        return await Delete(picture);
    }

    public async Task<Picture> Delete(Picture picture)
    {
        return (await Delete(new List<Picture> { picture })).FirstOrDefault();
    }

    public async Task DeleteByUser(Guid userId)
    {
        var pictures = await _ctx.Pictures.Where(x => x.UserId == userId).ToListAsync();
        await Delete(pictures);
    }

    public async Task<IEnumerable<Picture>> Delete(IList<Picture> pictures)
    {
        if (!pictures.Any())
        {
            return Enumerable.Empty<Picture>();
        }

        // Remove pictures from db
        await _ctx.DeleteRange(pictures);

        // Remove pictures from disk
        Parallel.ForEach(pictures, picture => PhysicalDelete(picture.Path));

        // Remove any comms about this picture
        _ = _commEvents?.DeleteNotifications(new(pictures.Select(x => x.PictureId.ToString())));

        return pictures;
    }

    private void PhysicalDelete(string path)
    {
        try
        {
            var filePath = _settings.UploadDir + path;
            File.Delete(filePath);

            filePath = $"{ _settings.UploadDir }thumbs/{ path }";
            File.Delete(filePath);
        }
        catch { }
    }

    private async Task<(Guid? pictureId, IList<ErrorCodeDto> errors)> Upload(IUser user, byte[] bytes, string fileName, string ipAddress)
    {
        // Verify upload access
        var settings = await VerifyUserCanUpload(user.UserId, 1);

        // Store
        var errors = new List<ErrorCodeDto>();
        var lat = (decimal)0;
        var lng = (decimal)0;
        var dateTaken = DateTime.UtcNow;
        var ext = Path.GetExtension(fileName).ToLower();

        // TODO: Undo for live
        // Check hash and reject duplicate hash
        var sha1 = System.Security.Cryptography.SHA1.Create();
        var hash = Convert.ToBase64String(sha1.ComputeHash(bytes));
        //if ((await _ctx.Pictures.FindDefaultAsync(p => p.Hash == hash)) != null)
        //{
        //    File.Delete(path);
        //    throw new SiteException(ErrorCode.DuplicatePicture);
        //}

        using var image = Image.Load(bytes);

        // Run checks on the picture
        //
        // Make sure file extension is valid
        var exts = new string[] { ".jpg", ".jpeg", ".png" };
        if (!exts.Contains(ext))
        {
            errors.Add(new ErrorCodeDto(ErrorCode.InvalidPictureExtension));
        }

        // Validate size
        var fileSizeBytes = settings.MaxPictureSize * 1000 * 1000;
        if (fileSizeBytes > 0 && bytes.Length > fileSizeBytes)
        {
            errors.Add(new ErrorCodeDto(ErrorCode.PictureTooBig, new string[] { $"{ settings.MaxPictureSize }mb" }));
        }

        // Validate dimensions
        if (image.Height <= settings.MinPictureResolutionX || image.Width <= settings.MinPictureResolutionY)
        {
            errors.Add(new ErrorCodeDto(ErrorCode.PictureDimensionsTooSmall));
        }

        // Validate exif info
        var profile = image.Metadata.ExifProfile;

        // Validate exif profile
        if (profile == null)
        {
            errors.Add(new ErrorCodeDto(ErrorCode.MissingExifLocation));
        }
        else
        {
            // Validate location
            (lat, lng) = profile.GetCoords();
            if (lat == 0 || lng == 0)
            {
                errors.Add(new ErrorCodeDto(ErrorCode.MissingExifLocation));
            }

            // Validate date
            var exifDate = profile.Values.FirstOrDefault(e => e.Tag == ExifTag.DateTimeOriginal);
            if (exifDate == null || !DateTime.TryParseExact(exifDate.GetValue().ToString(), "yyyy:MM:dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTaken))
            {
                errors.Add(new ErrorCodeDto(ErrorCode.MissingExifTimestamp));
            }
        }

        // Stop processing if there are errors
        if (errors.Count > 0)
        {
            return (null, errors);
        }

        // Find location within lat/lng bounds or add location request
        var location = await _locationQueries.WithinBounds(lat, lng);
        if (location == null && !await _ctx.PictureLocationRequests.AnyAsync(x => x.Lat == lat && x.Lng == lng))
        {
            await _ctx.Post(new PictureLocationRequest(lat, lng));
        }

        // Extract colour pallete
        var colours = new List<string>();
        try
        {
            // Get top colours
            var colourDict = new Dictionary<string, int>();
            for (var i = 0; i < image.Width; i++)
            {
                for (var j = 0; j < image.Height; j++)
                {
                    var pixel = image[i, j];
                    var shade = Functions.GetColourRange(pixel.R, pixel.G, pixel.B);
                    if (shade?.Length == 0)
                    {
                        continue;
                    }

                    if (colourDict.ContainsKey(shade)) colourDict[shade]++;
                    else colourDict.Add(shade, 1);
                }

                colours = colourDict
                    .Where(x => x.Value > 250)
                    .OrderByDescending(x => x.Value)
                    .Take(5)
                    .OrderBy(s => s.Value)
                    .Select(x => x.Key)
                    .ToList();
            }
        }
        catch { }

        // Create picture in the database
        var picture = new Picture
        {
            Name = fileName,
            Ext = ext,
            UserId = user.UserId,
            Username = user.Username,
            IpAddress = ipAddress,
            Format = ext,
            Width = image.Width,
            Height = image.Height,
            Lat = lat,
            Lng = lng,
            Hash = hash,
            Colours = string.Join(", ", colours),
            Location = location,
            DateTaken = dateTaken,
            Status = PictureStatus.Draft
        };
        picture = await _ctx.Post(picture);

        try
        {
            // Fix orientation
            image.Mutate(x => x.AutoOrient());

            // Resize to max requirements
            if (_settings.ResizeWidth > 0 && image.Width > _settings.ResizeWidth)
            {
                image.Mutate(x => x.Resize(_settings.ResizeWidth, 0));
            }

            // Resize to max requirements
            if (_settings.ResizeHeight > 0 && image.Height > _settings.ResizeHeight)
            {
                image.Mutate(x => x.Resize(0, _settings.ResizeHeight));
            }

            // Save images
            var saveTo = $"{ picture.PictureId }{ ext }";
            var imagePath = _settings.UploadDir + saveTo;
            if (!string.IsNullOrEmpty(_settings.UploadDir))
            {
                // Create directories
                var thumbsDir = $"{ _settings.UploadDir }thumbs";
                if (!Directory.Exists(_settings.UploadDir)) Directory.CreateDirectory(_settings.UploadDir);
                if (!Directory.Exists(thumbsDir)) Directory.CreateDirectory(thumbsDir);

                // Remove exif data
                image.Metadata.ExifProfile.RemoveCoords();

                // Save picture
                image.Save(imagePath);

                // Save thumbnail
                image.Mutate(x => x.Resize(400, 0));
                image.Save($"{ thumbsDir }/{ saveTo }");
            }

            // TODO: Scan for viruses
            //if (!Scanner.WindowsDefenderScan(imagePath))
            //{
            //    File.Delete(path);
            //    throw new SiteException(ErrorCode.DangerousPicture);
            //}

            // Update width and height in db
            picture.Width = image.Width;
            picture.Height = image.Height;

            picture = await _ctx.Put(picture);
        }
        catch
        {
            await _ctx.Delete(picture);
            throw;
        }

        return (picture.PictureId, null);
    }
}