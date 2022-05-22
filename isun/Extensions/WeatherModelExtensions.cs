using isun.Entities.Storage;
using isun.Models;

namespace isun.Extensions;

public static class WeatherModelExtensions
{
    public static WeatherDataEntity ToWeatherDataEntity(this WeatherModel model)
    {
        return new WeatherDataEntity(model.City, model.Temperature, model.Precipitation, model.WindSpeed,
            model.Summary);
    }
}