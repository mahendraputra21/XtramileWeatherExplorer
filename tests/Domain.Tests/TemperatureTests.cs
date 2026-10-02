using Api.Domain.ValueObjects;

namespace Domain.Tests;

public sealed class TemperatureTests
{
    [Theory]
    [InlineData(32, 0)]
    [InlineData(212, 100)]
    [InlineData(-40, -40)]
    [InlineData(98.6, 37)]
    [InlineData(68, 20)]
    public void ToCelsius_ConvertsCorrectly(double f, double expectedC)
    {
        var actual = Temperature.ToCelsius(f);
        Assert.Equal(expectedC, actual, precision: 2);
    }

    [Theory]
    [InlineData(0, 32)]
    [InlineData(100, 212)]
    [InlineData(-40, -40)]
    public void ToFahrenheit_ConvertsCorrectly(double c, double expectedF)
    {
        Assert.Equal(expectedF, Temperature.ToFahrenheit(c), precision: 2);
    }

    [Fact]
    public void ToCelsius_Negative40_IsEdgeCase()
    {
        Assert.Equal(-40, Temperature.ToCelsius(-40), precision: 5);
        Assert.Equal(-40, Temperature.ToFahrenheit(-40), precision: 5);
    }
    [Theory]
    [InlineData(25, 65, 17.95)]
    [InlineData(20, 50, 9.25)]
    [InlineData(30, 80, 26.16)]
    [InlineData(0, 100, 0.00)]
    public void CalcDewPointC_ReturnsExpected(double tempC, int humidity, double expectedDewC)
    {
        var actual = Temperature.CalcDewPointC(tempC, humidity);
        Assert.True(Math.Abs(expectedDewC - actual) < 0.12, $"expected {expectedDewC} got {actual}");
    }

    [Fact]
    public void CalcDewPointC_At100Percent_EqualsAirTemp()
    {
        Assert.Equal(25.0, Temperature.CalcDewPointC(25, 100), precision: 1);
        Assert.Equal(0.0, Temperature.CalcDewPointC(0, 100), precision: 1);
    }

    [Fact]
    public void CalcDewPointC_LowerHumidity_LowerDewPoint()
    {
        var high = Temperature.CalcDewPointC(25, 80);
        var low = Temperature.CalcDewPointC(25, 30);
        Assert.True(low < high);
    }

}
