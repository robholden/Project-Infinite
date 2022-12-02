namespace Content.Domain.Dtos;

public class CountryDto
{
    public string Name { get; set; }

    public string Code { get; set; }

    public decimal Lat { get; set; } = 0;

    public decimal Lng { get; set; } = 0;
}