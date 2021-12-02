
using Content.Core;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Content.Api.Controllers;

[Route("[controller]")]
public class ImagesController : Controller
{
    private readonly PictureSettings _settings;

    public ImagesController(IOptions<PictureSettings> options)
    {
        _settings = options.Value;
    }

    [HttpGet]
    public Task<IActionResult> Get([FromQuery] string name) => ReturnPicture(_settings.UploadDir + name);

    [HttpGet("thumbnail")]
    public Task<IActionResult> Thumbnail([FromQuery] string name) => ReturnPicture($"{ _settings.UploadDir }thumbs/{ name }");

    private async Task<IActionResult> ReturnPicture(string path)
    {
        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        var arrBytes = await System.IO.File.ReadAllBytesAsync(path);
        var ext = System.IO.Path.GetExtension(path);
        var contentType = "image/jpeg";

        switch (ext)
        {
            case ".png":
                contentType = "image/png";
                break;

            case ".gif":
                contentType = "image/gif";
                break;
        }

        return File(arrBytes, contentType);
    }
}