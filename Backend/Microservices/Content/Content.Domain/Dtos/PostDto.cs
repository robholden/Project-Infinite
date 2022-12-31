namespace Content.Domain.Dtos;

public class PostDto
{
    public Guid PostId { get; set; }

    public string Author { get; set; }

    public string Body { get; set; }

    public string Title { get; set; }

    public DateTime Created { get; set; }
}
