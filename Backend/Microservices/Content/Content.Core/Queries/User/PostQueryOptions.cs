using System.ComponentModel.DataAnnotations;

using Library.Core;

namespace Content.Core.Queries;

public class PostQueryOptions : IPageListQuery<PostQueryOptions.OrderByEnum>
{
    public enum OrderByEnum
    {
        None = 0,
        CreatedDate,
        UpdatedDate,
        Title,
        Username
    }

    [Flags]
    public enum FilterParams
    {
        None = 0,
        Title = 1 << 0,
        Body = 1 << 1
    }

    [MaxLength(50)]
    public string Username { get; set; }

    [MaxLength(255)]
    public string FilterText { get; set; }

    public FilterParams? FilterBy { get; set; }

    public OrderByEnum OrderBy { get; set; }
}