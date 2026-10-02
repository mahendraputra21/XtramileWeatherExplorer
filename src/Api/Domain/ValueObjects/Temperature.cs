namespace Api.Domain.ValueObjects;

/// <summary>Temperature conversion. Domain keeps the formula single-sourced.</summary>
public static class Temperature
{
    public static double ToCelsius(double fahrenheit) => (fahrenheit - 32) * 5.0 / 9.0;
    public static double ToFahrenheit(double celsius) => celsius * 9.0 / 5.0 + 32;

    /// <summary>Magnus formula — dew point in Celsius from air temp C and relative humidity % (1..100).</summary>
    public static double CalcDewPointC(double tempC, int humidity)
    {
        const double a = 17.27, b = 237.7;
        var gamma = a * tempC / (b + tempC) + Math.Log(humidity / 100.0);
        return b * gamma / (a - gamma);
    }
}
