using Api.Application.Common.Exceptions;
using Api.Application.Common.Interfaces;
using Api.Application.DTOs;
using MediatR;

namespace Api.Application.Features.Cities;

public sealed record GetCitiesByCountryQuery(string CountryCode) : IRequest<IReadOnlyList<CityDto>>;

public sealed class GetCitiesByCountryHandler : IRequestHandler<GetCitiesByCountryQuery, IReadOnlyList<CityDto>>
{
    private readonly ICountryRepository _countryRepo;
    private readonly ICityRepository _cityRepo;

    public GetCitiesByCountryHandler(ICountryRepository countryRepo, ICityRepository cityRepo)
    {
        _countryRepo = countryRepo;
        _cityRepo = cityRepo;
    }

    public async Task<IReadOnlyList<CityDto>> Handle(GetCitiesByCountryQuery request, CancellationToken ct)
    {
        var country = await _countryRepo.GetByCodeAsync(request.CountryCode, ct);
        if (country is null)
            throw new NotFoundException($"Country '{request.CountryCode}' not found.");

        var cities = await _cityRepo.GetByCountryCodeAsync(request.CountryCode, ct);
        return cities.Select(c => new CityDto(c.Id, c.Name, c.CountryCode)).ToList();
    }
}
