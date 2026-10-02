namespace Api.Domain.Entities;

public sealed class City
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string CountryCode { get; set; } = default!;
}
