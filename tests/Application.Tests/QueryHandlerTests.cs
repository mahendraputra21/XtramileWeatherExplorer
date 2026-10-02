using Api.Application.Features.Cities;
using Api.Application.Features.Countries;
using Api.Application.Common.Exceptions;
using Api.Infrastructure.Persistence;
using Api.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests;

public sealed class QueryHandlerTests
{
    private static AppDbContext NewSeededDb()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var db = new AppDbContext(opts);
        // replicate seed without calling EnsureSeeded
        db.Countries.AddRange(new[] {
            new Api.Domain.Entities.Country{ Code="AU", Name="Australia"},
            new Api.Domain.Entities.Country{ Code="ID", Name="Indonesia"},
        });
        db.Cities.AddRange(new[] {
            new Api.Domain.Entities.City{ Id=1, Name="Sydney", CountryCode="AU"},
            new Api.Domain.Entities.City{ Id=2, Name="Jakarta", CountryCode="ID"},
        });
        db.SaveChanges();
        return db;
    }

    [Fact]
    public async Task GetCountries_ReturnsSeeded()
    {
        using var db = NewSeededDb();
        var h = new GetCountriesHandler(new CountryRepository(db));
        var result = await h.Handle(new GetCountriesQuery(), CancellationToken.None);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetCities_ReturnsForCountry()
    {
        using var db = NewSeededDb();
        var h = new GetCitiesByCountryHandler(new CountryRepository(db), new CityRepository(db));
        var result = await h.Handle(new GetCitiesByCountryQuery("AU"), CancellationToken.None);
        Assert.Single(result);
        Assert.Equal("Sydney", result[0].Name);
    }

    [Fact]
    public async Task GetCities_ThrowsNotFound_ForUnknownCountry()
    {
        using var db = NewSeededDb();
        var h = new GetCitiesByCountryHandler(new CountryRepository(db), new CityRepository(db));
        await Assert.ThrowsAsync<NotFoundException>(() => h.Handle(new GetCitiesByCountryQuery("ZZ"), CancellationToken.None));
    }
}
