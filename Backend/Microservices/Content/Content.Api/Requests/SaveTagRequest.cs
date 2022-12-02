using Content.Domain.Dtos;

namespace Content.Api.Requests;

public class SaveTagRequest
{
    public IEnumerable<TagAdminDto> ToAdd { get; set; }

    public IEnumerable<TagAdminDto> ToUpdate { get; set; }

    public IEnumerable<int> ToDelete { get; set; }
}