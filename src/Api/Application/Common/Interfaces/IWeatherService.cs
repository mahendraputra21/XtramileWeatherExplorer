using Api.Application.DTOs;

namespace Api.Application.Common.Interfaces;

public interface IWeatherService
{
    Task<WeatherDto> GetCurrentWeatherAsync(string cityName, CancellationToken ct);
}
