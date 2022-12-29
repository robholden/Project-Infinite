using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Content.Domain.Dtos;

using Library.Core;

namespace Content.Domain;

public class Post
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid PostId { get; set; }

    public Guid UserId { get; set; }

    public string Author { get; set; }

    public string Body { get; set; }

    public string Title { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;

    public DateTime? Updated { get; set; }

    public Post() { }

    public Post(IUser user, PostDto post)
    {
        UserId = user.UserId;
        Author = user.Username;
        Body = post.Body;
        Title = post.Title;
    }
}
