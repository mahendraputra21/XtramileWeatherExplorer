using Api.Application.Features.Notes;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Persistence;
using Api.Infrastructure.Persistence.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests;

public sealed class CreateWeatherNoteHandlerTests
{
    private static AppDbContext NewDb()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new AppDbContext(opts);
    }

    [Fact]
    public async Task Handle_PersistsNote_AndConvertsTemp()
    {
        using var db = NewDb();
        var repo = new WeatherNoteRepository(db);
        var handler = new CreateWeatherNoteHandler(repo);

        var cmd = new CreateWeatherNoteCommand("Sydney", "AU", "Nice day", 77);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.Id > 0);
        var saved = await db.WeatherNotes.AsNoTracking().FirstAsync();
        Assert.Equal("Sydney", saved.CityName);
        Assert.Equal("AU", saved.CountryCode);
        Assert.Equal(77, saved.TemperatureF);
        Assert.Equal(Math.Round(Temperature.ToCelsius(77), 2), saved.TemperatureC);
    }

    [Fact]
    public async Task Validator_Fails_OnEmptyNote()
    {
        var v = new CreateWeatherNoteValidator();
        var result = await v.ValidateAsync(new CreateWeatherNoteCommand("Sydney", "AU", "", 77));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Note");
    }

    [Fact]
    public async Task Validator_Fails_OnBadCountryCode()
    {
        var v = new CreateWeatherNoteValidator();
        var r1 = await v.ValidateAsync(new CreateWeatherNoteCommand("Sydney", "", "hi", 77));
        Assert.False(r1.IsValid);
        var r2 = await v.ValidateAsync(new CreateWeatherNoteCommand("Sydney", "A", "hi", 77));
        Assert.False(r2.IsValid);
        var r3 = await v.ValidateAsync(new CreateWeatherNoteCommand("Sydney", "AUS", "hi", 77));
        Assert.False(r3.IsValid);
    }

    [Fact]
    public async Task Validator_Fails_OnEmptyCityName()
    {
        var v = new CreateWeatherNoteValidator();
        var r = await v.ValidateAsync(new CreateWeatherNoteCommand("", "AU", "hi", 77));
        Assert.False(r.IsValid);
    }
}
