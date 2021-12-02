using System.ComponentModel.DataAnnotations;

using Library.Core;

namespace Content.Core.Queries;

public class CollectionQueryOptions : IPageListQuery<CollectionQueryOptions.OrderByEnum>
{
    public enum OrderByEnum
    {
        None,
        Name
    }

    [MaxLength(200)]
    public string Name { get; set; }

    public Guid? UserId { get; set; }

    public string Username { get; set; }

    public Guid? IncludePictureId { get; set; }

    public OrderByEnum OrderBy { get; set; }
}