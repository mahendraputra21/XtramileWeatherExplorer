using System.Net.Http.Json;
using System.Text.Json;

namespace Web.Services;

public sealed class WeatherApiClient
{
    private readonly HttpClient _http;
    public WeatherApiClient(HttpClient http) => _http = http;

    public Task<List<CountryDto>?> GetCountriesAsync(CancellationToken ct = default)
        => _http.GetFromJsonAsync<List<CountryDto>>("api/countries", ct);

    public Task<List<CityDto>?> GetCitiesAsync(string countryCode, CancellationToken ct = default)
        => _http.GetFromJsonAsync<List<CityDto>>($"api/countries/{Uri.EscapeDataString(countryCode)}/cities", ct);

    public async Task<WeatherDto?> GetWeatherAsync(string cityName, CancellationToken ct = default)
    {
        var res = await _http.GetAsync($"api/weather/{Uri.EscapeDataString(cityName)}", ct);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadFromJsonAsync<WeatherDto>(cancellationToken: ct);
    }

    public async Task<(bool ok, string? error, int? id)> CreateNoteAsync(string city, string countryCode, string note, double tempF, CancellationToken ct = default)
    {
        var res = await _http.PostAsJsonAsync("api/weather/notes", new { cityName = city, countryCode, note, temperatureF = tempF }, ct);
        if (res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync(ct);
            try { var doc = JsonDocument.Parse(body); var id = doc.RootElement.GetProperty("id").GetInt32(); return (true, null, id); } catch { return (true, null, null); }
        }
        var err = await res.Content.ReadAsStringAsync(ct);
        return (false, err, null);
    }
}

public sealed record CountryDto(string Code, string Name);
public sealed record CityDto(int Id, string Name, string CountryCode);
public sealed record WeatherDto(LocationDto Location, DateTime TimeUtc, WindDto Wind, int Visibility, int Pressure, string Sky, string Description, TemperatureDto Temperature, TemperatureDto DewPoint, int Humidity);
public sealed record LocationDto(string City, string Country, string CountryCode);
public sealed record WindDto(double Speed, int Direction);
public sealed record TemperatureDto(double Fahrenheit, double Celsius);
