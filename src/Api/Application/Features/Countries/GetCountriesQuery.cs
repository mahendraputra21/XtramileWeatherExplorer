using Api.Application.Common.Interfaces;
using Api.Application.DTOs;
using MediatR;

namespace Api.Application.Features.Countries;

public sealed record GetCountriesQuery : IRequest<IReadOnlyList<CountryDto>>;

public sealed class GetCountriesHandler : IRequestHandler<GetCountriesQuery, IReadOnlyList<CountryDto>>
{
    private readonly ICountryRepository _repo;
    public GetCountriesHandler(ICountryRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<CountryDto>> Handle(GetCountriesQuery request, CancellationToken ct)
    {
        var countries = await _repo.GetAllAsync(ct);
        return countries.Select(c => new CountryDto(c.Code, c.Name)).ToList();
    }
}
