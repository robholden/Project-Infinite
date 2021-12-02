using System.ComponentModel.DataAnnotations;

using Library.Core;

namespace Identity.Core.Queries;

public class AuthTokenQueryOptions : IPageListQuery<AuthTokenQueryOptions.OrderByEnum>
{
    public enum OrderByEnum
    {
        None = 0,
        CreationDate,
        RefreshedDate,
        UpdatedDate
    }

    public bool? Active { get; set; }

    public DateTime? ActiveDate { get; set; }

    [MaxLength(50)]
    public string IpAddress { get; set; }

    public Guid? UserId { get; set; }

    public Guid? IgnoreAccessKey { get; set; }

    public OrderByEnum OrderBy { get; set; }
}