using isun.Entities.Http;
using isun.Models;

namespace isun.Extensions;

public static class WeatherEntityExtensions
{
    public static WeatherModel ToWeatherModel(this WeatherEntity entity)
    {
        return new WeatherModel(entity.City, entity.Temperature, entity.Precipitation, entity.WindSpeed,
            entity.Summary);
    }
}