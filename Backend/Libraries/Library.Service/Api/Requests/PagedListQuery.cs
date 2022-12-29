using Microsoft.AspNetCore.Mvc;

namespace Library.Service.Api.Requests;

public class PagedListQuery
{
    [FromQuery]
    public int page { get; set; } = 1;

    [FromQuery]
    public int pageSize { get; set; } = 10;

    [FromQuery]
    public string orderDir { get; set; } = null;
}
