using System.ComponentModel.DataAnnotations;

using Library.Core;

namespace Content.Core.Queries;

public class PictureModerationQueryOptions : IPageListQuery<PictureModerationQueryOptions.OrderByEnum>
{
    public enum OrderByEnum
    {
        None,
        Username,
        Date
    }

    [MaxLength(255)]
    public string Username { get; set; }

    public OrderByEnum OrderBy { get; set; }
}