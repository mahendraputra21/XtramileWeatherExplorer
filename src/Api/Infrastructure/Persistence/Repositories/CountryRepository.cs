using Api.Application.Common.Interfaces;
using Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Persistence.Repositories;

public sealed class CountryRepository : ICountryRepository
{
    private readonly AppDbContext _db;
    public CountryRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken ct)
        => await _db.Countries.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);

    public async Task<Country?> GetByCodeAsync(string code, CancellationToken ct)
        => await _db.Countries.AsNoTracking().FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant(), ct);
}
