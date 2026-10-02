using Api.Application.Common.Interfaces;
using Api.Application.DTOs;
using MediatR;

namespace Api.Application.Features.Weather;

public sealed record GetWeatherQuery(string CityName) : IRequest<WeatherDto>;

public sealed class GetWeatherHandler : IRequestHandler<GetWeatherQuery, WeatherDto>
{
    private readonly IWeatherService _weather;

    public GetWeatherHandler(IWeatherService weather) => _weather = weather;

    public Task<WeatherDto> Handle(GetWeatherQuery request, CancellationToken ct)
        => _weather.GetCurrentWeatherAsync(request.CityName, ct);
}
