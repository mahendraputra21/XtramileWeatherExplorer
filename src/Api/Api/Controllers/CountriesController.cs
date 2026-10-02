using Api.Application.Features.Cities;
using Api.Application.Features.Countries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/countries")]
public sealed class CountriesController : ControllerBase
{
    private readonly IMediator _mediator;
    public CountriesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetCountries(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCountriesQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{countryCode}/cities")]
    public async Task<IActionResult> GetCities(string countryCode, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCitiesByCountryQuery(countryCode), ct);
        return Ok(result);
    }
}
