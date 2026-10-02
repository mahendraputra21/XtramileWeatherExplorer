using Api.Application.Common.Interfaces;
using Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Persistence.Repositories;

public sealed class WeatherNoteRepository : IWeatherNoteRepository
{
    private readonly AppDbContext _db;
    public WeatherNoteRepository(AppDbContext db) => _db = db;

    public async Task<WeatherNote> AddAsync(WeatherNote note, CancellationToken ct)
    {
        var entry = await _db.WeatherNotes.AddAsync(note, ct);
        await _db.SaveChangesAsync(ct);
        return entry.Entity;
    }

    public async Task<IReadOnlyList<WeatherNote>> GetAllAsync(CancellationToken ct)
        => await _db.WeatherNotes.AsNoTracking().OrderByDescending(n => n.CreatedUtc).ToListAsync(ct);
}
