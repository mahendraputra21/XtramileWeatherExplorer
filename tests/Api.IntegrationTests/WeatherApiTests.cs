using System.Net;
using System.Net.Http.Json;
using Api.Application.Common.Interfaces;
using Api.Application.DTOs;
using Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace IntegrationTests;

public sealed class CustomFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "TestDb-" + Guid.NewGuid();
    private bool _seeded;
    private readonly object _lock = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(_dbName));

            var mock = new Mock<IWeatherService>();
            mock.Setup(s => s.GetCurrentWeatherAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string city, CancellationToken ct) =>
                    new WeatherDto(
                        new LocationDto(city, "AU", "AU"),
                        DateTime.UtcNow,
                        new WindDto(5.2, 180),
                        10000, 1012, "Clear", "clear sky",
                        new TemperatureDto(77, 25),
                        new TemperatureDto(59, 15),
                        65));

            services.RemoveAll(typeof(IWeatherService));
            services.AddSingleton<IWeatherService>(mock.Object);
        });
    }

    public void EnsureSeeded()
    {
        lock (_lock)
        {
            if (_seeded) return;
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (!db.Countries.Any())
            {
                db.Countries.AddRange(
                    new Api.Domain.Entities.Country{ Code="AU", Name="Australia"},
                    new Api.Domain.Entities.Country{ Code="ID", Name="Indonesia"},
                    new Api.Domain.Entities.Country{ Code="US", Name="United States"},
                    new Api.Domain.Entities.Country{ Code="JP", Name="Japan"});
                db.Cities.AddRange(
                    new Api.Domain.Entities.City{ Id=1, Name="Sydney", CountryCode="AU"},
                    new Api.Domain.Entities.City{ Id=2, Name="Melbourne", CountryCode="AU"},
                    new Api.Domain.Entities.City{ Id=5, Name="Jakarta", CountryCode="ID"},
                    new Api.Domain.Entities.City{ Id=13, Name="Tokyo", CountryCode="JP"});
                db.SaveChanges();
            }
            _seeded = true;
        }
    }
}

public sealed class NotFoundWeatherFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "TestDb-NF-" + Guid.NewGuid();
    private bool _seeded;
    private readonly object _lock = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(_dbName));

            var mock = new Mock<IWeatherService>();
            mock.Setup(s => s.GetCurrentWeatherAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Api.Application.Common.Exceptions.NotFoundException("City not found"));

            services.RemoveAll(typeof(IWeatherService));
            services.AddSingleton<IWeatherService>(mock.Object);
        });
    }

    public void EnsureSeeded()
    {
        lock (_lock)
        {
            if (_seeded) return;
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (!db.Countries.Any())
            {
                db.Countries.Add(new Api.Domain.Entities.Country{ Code="AU", Name="Australia"});
                db.Cities.Add(new Api.Domain.Entities.City{ Id=1, Name="Sydney", CountryCode="AU"});
                db.SaveChanges();
            }
            _seeded = true;
        }
    }
}

public sealed class ExternalFailureFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "TestDb-EF-" + Guid.NewGuid();
    private bool _seeded;
    private readonly object _lock = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(_dbName));

            var mock = new Mock<IWeatherService>();
            mock.Setup(s => s.GetCurrentWeatherAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Api.Application.Common.Exceptions.ExternalServiceException("upstream down"));

            services.RemoveAll(typeof(IWeatherService));
            services.AddSingleton<IWeatherService>(mock.Object);
        });
    }

    public void EnsureSeeded()
    {
        lock (_lock)
        {
            if (_seeded) return;
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (!db.Countries.Any())
            {
                db.Countries.Add(new Api.Domain.Entities.Country{ Code="AU", Name="Australia"});
                db.Cities.Add(new Api.Domain.Entities.City{ Id=1, Name="Sydney", CountryCode="AU"});
                db.SaveChanges();
            }
            _seeded = true;
        }
    }
}

public sealed class WeatherApiTests : IClassFixture<CustomFactory>
{
    private readonly CustomFactory _factory;
    public WeatherApiTests(CustomFactory factory)
    {
        _factory = factory;
        _factory.EnsureSeeded();
    }

    [Fact]
    public async Task GetCountries_Returns200()
    {
        var client = _factory.CreateClient();
        var res = await client.GetAsync("api/countries");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var data = await res.Content.ReadFromJsonAsync<List<CountryDto>>();
        Assert.NotNull(data);
        Assert.True(data.Count >= 4);
    }

    [Fact]
    public async Task GetCities_Returns200()
    {
        var client = _factory.CreateClient();
        var ok = await client.GetAsync("api/countries/AU/cities");
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        var cities = await ok.Content.ReadFromJsonAsync<List<CityDto>>();
        Assert.NotNull(cities);
        Assert.NotEmpty(cities);
    }

    [Fact]
    public async Task GetCities_UnknownCountry_Returns404()
    {
        var client = _factory.CreateClient();
        var res = await client.GetAsync("api/countries/ZZ/cities");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task GetWeather_Returns200()
    {
        var client = _factory.CreateClient();
        var res = await client.GetAsync("api/weather/Sydney");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var dto = await res.Content.ReadFromJsonAsync<WeatherDto>();
        Assert.NotNull(dto);
        Assert.Equal("Sydney", dto.Location.City);
        Assert.Equal(77, dto.Temperature.Fahrenheit);
    }

    [Fact]
    public async Task GetWeather_NotFound_MapsTo404()
    {
        using var factory = new NotFoundWeatherFactory();
        factory.EnsureSeeded();
        var client = factory.CreateClient();
        var res = await client.GetAsync("api/weather/UnknownCity123");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task GetWeather_ExternalFailure_MapsTo502()
    {
        using var factory = new ExternalFailureFactory();
        factory.EnsureSeeded();
        var client = factory.CreateClient();
        var res = await client.GetAsync("api/weather/Sydney");
        Assert.Equal(HttpStatusCode.BadGateway, res.StatusCode);
    }

    [Fact]
    public async Task PostNote_Valid_Returns201()
    {
        var client = _factory.CreateClient();
        var payload = new { cityName = "Sydney", countryCode = "AU", note = "Lovely day", temperatureF = 77.0 };
        var res = await client.PostAsJsonAsync("api/weather/notes", payload);
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(body);
        Assert.True(body.ContainsKey("id"));
    }

    [Fact]
    public async Task PostNote_ValidationFailure_Returns400()
    {
        var client = _factory.CreateClient();
        var payload = new { cityName = "Sydney", countryCode = "AU", note = "", temperatureF = 77.0 };
        var res = await client.PostAsJsonAsync("api/weather/notes", payload);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task PostNote_MissingCity_Returns400()
    {
        var client = _factory.CreateClient();
        var payload = new { cityName = "", countryCode = "AU", note = "hi", temperatureF = 77.0 };
        var res = await client.PostAsJsonAsync("api/weather/notes", payload);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }
}
