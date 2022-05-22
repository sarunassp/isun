using isun.Extensions;
using isun.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace isun.Repositories;

public interface IWeatherRepository
{
    Task SaveWeatherData(IReadOnlyCollection<WeatherModel> weatherData);
}

public class WeatherRepository : IWeatherRepository
{
    private readonly ILogger<WeatherRepository> _logger;

    public WeatherRepository(ILogger<WeatherRepository> logger)
    {
        _logger = logger;
    }

    public async Task SaveWeatherData(IReadOnlyCollection<WeatherModel> weatherData)
    {
        try
        {
            var currentDir = Directory.GetCurrentDirectory();
            Directory.CreateDirectory($"{currentDir}/weatherData/");
            await File.WriteAllTextAsync(
                $"{currentDir}/weatherData/{((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds()}_{Guid.NewGuid()}.json",
                JsonConvert.SerializeObject(weatherData.Select(a => a.ToWeatherDataEntity())));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to save data for {NumberOfCities} cities", weatherData.Count);
            return;
        }
        _logger.LogInformation("Saved weather data for {NumberOfCities} cities", weatherData.Count);
    }
}