using Api.Application.Common.Interfaces;
using Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Persistence.Repositories;

public sealed class CityRepository : ICityRepository
{
    private readonly AppDbContext _db;
    public CityRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<City>> GetByCountryCodeAsync(string countryCode, CancellationToken ct)
        => await _db.Cities.AsNoTracking()
            .Where(c => c.CountryCode == countryCode.ToUpperInvariant())
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public async Task<City?> GetByNameAsync(string cityName, CancellationToken ct)
        => await _db.Cities.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name.ToLower() == cityName.ToLower(), ct);

    public async Task<bool> ExistsAsync(string cityName, CancellationToken ct)
        => await _db.Cities.AsNoTracking().AnyAsync(c => c.Name.ToLower() == cityName.ToLower(), ct);
}
