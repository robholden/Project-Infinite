namespace Content.Api.Results;

public class UserStatsResult
{
    public UserStatsResult()
    {
    }

    public UserStatsResult(int drafts, int pending, int published, int collections, int likes)
    {
        Drafts = drafts;
        Pending = pending;
        Published = published;
        Likes = likes;
        Collections = collections;
    }

    public int Drafts { get; set; }

    public int Pending { get; set; }

    public int Published { get; set; }

    public int Collections { get; set; }

    public int Likes { get; set; }
}