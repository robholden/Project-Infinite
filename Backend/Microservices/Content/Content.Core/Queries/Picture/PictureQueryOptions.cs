
using Content.Domain;

using Library.Core;

namespace Content.Core.Queries;

public class PictureQueryOptions : IPageListQuery<PictureQueryOptions.OrderByEnum>
{
    public enum OrderByEnum
    {
        None,
        UploadDate,
        Name,
        DateTaken
    }

    public string Seed { get; set; }

    public string Name { get; set; }

    public string[] Locations { get; set; }

    public string[] Countries { get; set; }

    public int? Distance { get; set; }

    public string Username { get; set; }

    public bool ShowLikes { get; set; }

    public Guid? CollectionId { get; set; }

    public PictureStatus? Status { get; set; }

    public bool Draft { get; set; }

    public string[] Tags { get; set; }

    public OrderByEnum OrderBy { get; set; }
}