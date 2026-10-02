using Api.Application.DTOs;
using Api.Application.Features.Weather;
using Api.Application.Common.Interfaces;
using Moq;

namespace Application.Tests;

public sealed class GetWeatherQueryHandlerTests
{
    [Fact]
    public async Task Handle_DelegatesToWeatherService()
    {
        var expected = new WeatherDto(
            new LocationDto("Sydney", "AU", "AU"), DateTime.UtcNow, new WindDto(5, 180),
            10000, 1012, "Clear", "clear sky", new TemperatureDto(77, 25), new TemperatureDto(59, 15), 65);

        var mock = new Mock<IWeatherService>();
        mock.Setup(s => s.GetCurrentWeatherAsync("Sydney", It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var handler = new GetWeatherHandler(mock.Object);
        var result = await handler.Handle(new GetWeatherQuery("Sydney"), CancellationToken.None);

        Assert.Equal(expected, result);
        mock.Verify(s => s.GetCurrentWeatherAsync("Sydney", It.IsAny<CancellationToken>()), Times.Once);
    }
}
