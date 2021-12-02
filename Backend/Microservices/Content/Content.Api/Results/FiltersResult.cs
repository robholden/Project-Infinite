
using Content.Api.Dtos;

namespace Content.Api.Results;

public class FiltersResult
{
    public FiltersResult() { }

    public FiltersResult(IEnumerable<LocationDto> locations, IEnumerable<TagDto> tags)
    {
        Locations = locations;
        Tags = tags;
    }

    public IEnumerable<LocationDto> Locations { get; set; }

    public IEnumerable<TagDto> Tags { get; set; }
}