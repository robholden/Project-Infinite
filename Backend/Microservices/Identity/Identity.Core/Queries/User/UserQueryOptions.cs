using System.ComponentModel.DataAnnotations;

using Library.Core;

namespace Identity.Core.Queries;

public class UserQueryOptions : IPageListQuery<UserQueryOptions.OrderByEnum>
{
    public enum OrderByEnum
    {
        None = 0,
        CreationDate,
        LastActive,
        Email,
        Name,
        Username
    }

    public Guid? UserId { get; set; }

    [MaxLength(50)]
    public string Username { get; set; }

    [MaxLength(255)]
    public string Name { get; set; }

    [EmailAddress, MaxLength(500)]
    public string EmailAddress { get; set; }

    [MaxLength(50)]
    public string Mobile { get; set; }

    public bool? Active { get; set; }

    public OrderByEnum OrderBy { get; set; }
}