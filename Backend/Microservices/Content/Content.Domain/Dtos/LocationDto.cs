namespace Content.Domain.Dtos;

public class LocationDto
{
    public Guid LocationId { get; set; }

    public string Name { get; set; }

    public string Code { get; set; }

    public decimal Lat { get; set; }

    public decimal Lng { get; set; }

    public CountryDto Country { get; set; }

    public BoundryDto Boundry { get; set; }
}