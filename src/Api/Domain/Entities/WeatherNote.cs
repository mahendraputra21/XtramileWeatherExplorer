namespace Api.Domain.Entities;

public sealed class WeatherNote
{
    public int Id { get; set; }
    public string CityName { get; set; } = default!;
    public string CountryCode { get; set; } = default!;
    public string Note { get; set; } = default!;
    public double TemperatureF { get; set; }
    public double TemperatureC { get; set; }
    public DateTime CreatedUtc { get; set; }
}
