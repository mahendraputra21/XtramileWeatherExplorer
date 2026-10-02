using Api.Domain.Entities;

namespace Api.Application.Common.Interfaces;

public interface IWeatherNoteRepository
{
    Task<WeatherNote> AddAsync(WeatherNote note, CancellationToken ct);
    Task<IReadOnlyList<WeatherNote>> GetAllAsync(CancellationToken ct);
}
