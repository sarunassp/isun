namespace isun.Models;

public class WeatherModel
{
    public WeatherModel(string city, int temperature, int precipitation, int windSpeed, string summary)
    {
        City = city;
        Temperature = temperature;
        Precipitation = precipitation;
        WindSpeed = windSpeed;
        Summary = summary;
    }

    public WeatherModel(string city, string message)
    {
        City = city;
        Message = message;
    }

    public string City { get; }

    public int Temperature { get; }

    public int Precipitation { get; }

    public int WindSpeed { get; }

    public string Summary { get; }

    public string Message { get; }

    public bool HasWeatherInformation => Message == null;
}