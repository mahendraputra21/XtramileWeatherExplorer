using Api.Domain.Entities;

namespace Api.Application.Common.Interfaces;

public interface ICountryRepository
{
    Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken ct);
    Task<Country?> GetByCodeAsync(string code, CancellationToken ct);
}
