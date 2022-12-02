using Content.Domain.Dtos;

using Library.Core;

namespace Content.Api.Results;

public class UploadPictureResult
{
    public PictureUserDto Picture { get; set; }

    public IList<ErrorCodeDto> Errors { get; set; }
}