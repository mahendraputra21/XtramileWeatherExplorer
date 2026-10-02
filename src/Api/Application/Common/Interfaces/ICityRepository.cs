using Api.Domain.Entities;

namespace Api.Application.Common.Interfaces;

public interface ICityRepository
{
    Task<IReadOnlyList<City>> GetByCountryCodeAsync(string countryCode, CancellationToken ct);
    Task<City?> GetByNameAsync(string cityName, CancellationToken ct);
    Task<bool> ExistsAsync(string cityName, CancellationToken ct);
}
