using System.Net;
using System.Text.Json;
using Api.Application.Common.Exceptions;
using Api.Application.Common.Interfaces;
using Api.Application.DTOs;
using Api.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Api.Infrastructure.External;

public sealed class OpenWeatherMapService : IWeatherService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<OpenWeatherMapService> _logger;

    public OpenWeatherMapService(HttpClient http, IConfiguration config, ILogger<OpenWeatherMapService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task<WeatherDto> GetCurrentWeatherAsync(string cityName, CancellationToken ct)
    {
        var apiKey = _config["OpenWeatherMap:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ExternalServiceException("OpenWeatherMap API key not configured (OpenWeatherMap:ApiKey).");

        var url = $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(cityName)}&appid={apiKey}&units=imperial";

        HttpResponseMessage res;
        try
        {
            res = await _http.GetAsync(url, ct);
        }
        catch (Exception ex) when (ex is not ExternalServiceException and not NotFoundException)
        {
            throw new ExternalServiceException("Failed to call OpenWeatherMap.", ex);
        }

        if (res.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException($"City/weather '{cityName}' not found.");

        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("OWM error {Status} {Body}", (int)res.StatusCode, body);
            throw new ExternalServiceException($"Weather service returned {(int)res.StatusCode}.");
        }

        var json = await res.Content.ReadAsStringAsync(ct);
        return MapToDto(json, cityName);
    }

    private static WeatherDto MapToDto(string json, string requestedCity)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var city = root.TryGetProperty("name", out var n) ? n.GetString() ?? requestedCity : requestedCity;
        var country = root.TryGetProperty("sys", out var sys) && sys.TryGetProperty("country", out var cc) ? cc.GetString() ?? "" : "";
        var dt = root.TryGetProperty("dt", out var dtEl) ? dtEl.GetInt64() : DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var timeUtc = DateTimeOffset.FromUnixTimeSeconds(dt).UtcDateTime;

        var main = root.GetProperty("main");
        var tempF = main.GetProperty("temp").GetDouble();
        var pressure = main.GetProperty("pressure").GetInt32();
        var humidity = main.GetProperty("humidity").GetInt32();

        var windEl = root.TryGetProperty("wind", out var w) ? w : default;
        var windSpeed = windEl.ValueKind != JsonValueKind.Undefined && windEl.TryGetProperty("speed", out var ws) ? ws.GetDouble() : 0;
        var windDeg = windEl.ValueKind != JsonValueKind.Undefined && windEl.TryGetProperty("deg", out var wd) ? wd.GetInt32() : 0;

        var visibility = root.TryGetProperty("visibility", out var vis) ? vis.GetInt32() : 10000;
        var weatherArr = root.TryGetProperty("weather", out var wa) && wa.GetArrayLength() > 0 ? wa[0] : default;
        var sky = weatherArr.ValueKind != JsonValueKind.Undefined && weatherArr.TryGetProperty("main", out var sm) ? sm.GetString() ?? "Unknown" : "Unknown";
        var desc = weatherArr.ValueKind != JsonValueKind.Undefined && weatherArr.TryGetProperty("description", out var sd) ? sd.GetString() ?? sky : sky;

        var tempC = Math.Round(Temperature.ToCelsius(tempF), 2);

        var dewC = Temperature.CalcDewPointC(tempC, humidity);
        var dewF = Math.Round(Temperature.ToFahrenheit(dewC), 2);

        return new WeatherDto(
            new LocationDto(city, country, country),
            timeUtc,
            new WindDto(windSpeed, windDeg),
            visibility,
            pressure,
            sky,
            desc,
            new TemperatureDto(Math.Round(tempF, 2), tempC),
            new TemperatureDto(dewF, Math.Round(dewC, 2)),
            humidity
        );
    }
}
