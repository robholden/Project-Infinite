
using Content.Domain;

using Library.Core;
using Library.Core.Enums;
using Library.Core.Models;

using Microsoft.AspNetCore.Http;

namespace Content.Core.Services;

public interface IPictureService
{
    Task<UserSetting> VerifyUserCanUpload(Guid userId, int uploadCount);

    Task<(Guid? pictureId, IList<ErrorCodeDto> errors)> Upload(IUser user, string path, string ipAddress);

    Task<(Guid? pictureId, IList<ErrorCodeDto> errors)> Upload(IUser user, IFormFile file, string ipAddress);

    Task Update(Guid pictureId, string name, IEnumerable<string> tags, bool concealCoords, UserLevel userLevel, string seed = null);

    Task Submit(Guid pictureId);

    Task<PictureModeration> NextModeration(string username);

    Task Moderate(Guid pictureId, IUser moderatedBy, bool outcome, string notes);

    Task Like(Guid pictureId, IUser triggeredUser, bool liked);

    Task<Picture> Delete(Guid pictureId);

    Task<Picture> Delete(Picture picture);

    Task DeleteByUser(Guid userId);

    Task<IEnumerable<Picture>> Delete(IList<Picture> pictures);
}