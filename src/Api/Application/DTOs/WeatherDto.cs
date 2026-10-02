namespace Api.Application.DTOs;

public sealed record WeatherDto(
    LocationDto Location,
    DateTime TimeUtc,
    WindDto Wind,
    int Visibility,
    int Pressure,
    string Sky,
    string Description,
    TemperatureDto Temperature,
    TemperatureDto DewPoint,
    int Humidity
);

public sealed record LocationDto(string City, string Country, string CountryCode);
public sealed record WindDto(double Speed, int Direction);
public sealed record TemperatureDto(double Fahrenheit, double Celsius);
