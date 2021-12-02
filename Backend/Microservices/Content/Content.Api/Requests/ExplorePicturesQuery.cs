
using Microsoft.AspNetCore.Mvc;

namespace Content.Api.Requests;

public class ExplorePicturesQuery
{
    [FromQuery]
    public string Locations { get; set; }

    [FromQuery]
    public string Countries { get; set; }

    [FromQuery]
    public int? Distance { get; set; }

    [FromQuery]
    public string Tags { get; set; }

    [FromQuery]
    public string OrderBy { get; set; }

    [FromQuery]
    public string OrderDir { get; set; }

    [FromQuery]
    public int Page { get; set; } = 1;

    [FromQuery]
    public int PageSize { get; set; } = 10;

    public bool HasFilter() => Locations?.Length > 0 || Countries?.Length > 0 || Tags?.Length > 0;
}
