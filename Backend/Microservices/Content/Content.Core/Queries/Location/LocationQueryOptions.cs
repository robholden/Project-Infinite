using System.ComponentModel.DataAnnotations;

using Library.Core;

namespace Content.Core.Queries;

public class LocationQueryOptions : IPageListQuery<LocationQueryOptions.OrderByEnum>
{
    public enum OrderByEnum
    {
        None,
        Name
    }

    [MaxLength(255)]
    public string Name { get; set; }

    public OrderByEnum OrderBy { get; set; }
}