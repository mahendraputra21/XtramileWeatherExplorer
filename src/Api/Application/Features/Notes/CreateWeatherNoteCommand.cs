using Api.Application.Common.Interfaces;
using Api.Application.DTOs;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Api.Application.Features.Notes;

public sealed record CreateWeatherNoteCommand(string CityName, string CountryCode, string Note, double TemperatureF) : IRequest<CreateWeatherNoteDto>;

public sealed class CreateWeatherNoteValidator : AbstractValidator<CreateWeatherNoteCommand>
{
    public CreateWeatherNoteValidator()
    {
        RuleFor(x => x.CityName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CountryCode).NotEmpty().Length(2);
        RuleFor(x => x.Note).NotEmpty().MaximumLength(500);
    }
}

public sealed class CreateWeatherNoteHandler : IRequestHandler<CreateWeatherNoteCommand, CreateWeatherNoteDto>
{
    private readonly IWeatherNoteRepository _repo;

    public CreateWeatherNoteHandler(IWeatherNoteRepository repo)
    {
        _repo = repo;
    }

    public async Task<CreateWeatherNoteDto> Handle(CreateWeatherNoteCommand request, CancellationToken ct)
    {
        var entity = new WeatherNote
        {
            CityName = request.CityName,
            CountryCode = request.CountryCode.ToUpperInvariant(),
            Note = request.Note,
            TemperatureF = request.TemperatureF,
            TemperatureC = Math.Round(Temperature.ToCelsius(request.TemperatureF), 2),
            CreatedUtc = DateTime.UtcNow
        };

        var saved = await _repo.AddAsync(entity, ct);
        return new CreateWeatherNoteDto(saved.Id);
    }
}
