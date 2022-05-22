namespace isun.Entities.Http;

public class WeatherEntity
{
    public WeatherEntity(string city, int temperature, int precipitation, int windSpeed, string summary)
    {
        City = city;
        Temperature = temperature;
        Precipitation = precipitation;
        WindSpeed = windSpeed;
        Summary = summary;
    }

    public string City { get; }

    public int Temperature { get; }

    public int Precipitation { get; }

    public int WindSpeed { get; }

    public string Summary { get; }
}