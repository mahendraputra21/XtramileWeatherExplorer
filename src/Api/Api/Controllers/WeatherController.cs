using Api.Application.DTOs;
using Api.Application.Features.Notes;
using Api.Application.Features.Weather;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/weather")]
public sealed class WeatherController : ControllerBase
{
    private readonly IMediator _mediator;
    public WeatherController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{cityName}")]
    public async Task<IActionResult> GetWeather(string cityName, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetWeatherQuery(cityName), ct);
        return Ok(result);
    }

    [HttpPost("notes")]
    public async Task<IActionResult> CreateNote([FromBody] CreateWeatherNoteRequest body, CancellationToken ct)
    {
        var cmd = new CreateWeatherNoteCommand(body.CityName, body.CountryCode, body.Note, body.TemperatureF);
        var result = await _mediator.Send(cmd, ct);
        return CreatedAtAction(null, new { id = result.Id }, result);
    }
}

public sealed record CreateWeatherNoteRequest(string CityName, string CountryCode, string Note, double TemperatureF);
