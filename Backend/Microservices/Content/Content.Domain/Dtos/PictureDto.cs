namespace Content.Domain.Dtos;

public class PictureDto
{
    protected decimal lat;
    protected decimal lng;

    public Guid PictureId { get; set; }

    public string Username { get; set; }

    public string Name { get; set; }

    public string Ext { get; set; }

    public string Path { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public bool ConcealCoords { get; set; }

    public decimal Lat { get => ConcealCoords ? 0 : lat; set => lat = value; }

    public decimal Lng { get => ConcealCoords ? 0 : lng; set => lng = value; }

    public IEnumerable<string> Tags { get; set; }

    public LocationDto Location { get; set; }

    public DateTime DateTaken { get; set; }

    public bool Liked { get; set; }

    public int LikesTotal { get; set; }
}

public class PictureUserDto : PictureDto
{
    public string Seed { get; set; }

    public Coords Coords => new(lat, lng);

    public DateTime CreatedDate { get; set; }

    public PictureStatus Status { get; set; }
}