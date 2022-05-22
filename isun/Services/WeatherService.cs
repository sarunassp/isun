using System.Collections.Concurrent;
using isun.Extensions;
using isun.Http.Clients;
using isun.Models;
using isun.Repositories;
using Microsoft.Extensions.Logging;
using MoreLinq;

namespace isun.Services;

public interface IWeatherService
{
    Task<IEnumerable<WeatherModel>> GetWeatherAsync(IEnumerable<string> requestedCities);
}

public class WeatherService : IWeatherService
{
    private readonly IWeatherClient _weatherClient;
    private readonly IWeatherRepository _weatherRepository;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(IWeatherClient weatherClient, IWeatherRepository weatherRepository, ILogger<WeatherService> logger)
    {
        _weatherClient = weatherClient;
        _weatherRepository = weatherRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<WeatherModel>> GetWeatherAsync(IEnumerable<string> requestedCities)
    {
        var availableCities = await _weatherClient.GetCitiesAsync();
        var availableCitiesHashSet = new HashSet<string>(availableCities);

        var cityWeather = new ConcurrentBag<WeatherModel>();
        var requestedCitiesBatches = requestedCities.Batch(5);
        foreach (var requestedCityBatch in requestedCitiesBatches)
        {
            var tasks = requestedCityBatch.Select(async (requestedCity) =>
            {
                if (availableCitiesHashSet.Contains(requestedCity) == false)
                {
                    _logger.LogWarning("{City} is not in the list of available cities", requestedCity);
                    cityWeather.Add(new WeatherModel(requestedCity, "Weather data not available for city"));
                    return;
                }

                try
                {
                    var weather = await _weatherClient.GetWeatherAsync(requestedCity);
                    cityWeather.Add(weather.ToWeatherModel());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get weather data for {City}", requestedCity);
                    cityWeather.Add(new WeatherModel(requestedCity, "Failed to get weather data for city"));
                }
            });

            await Task.WhenAll(tasks);
        }
        await _weatherRepository.SaveWeatherData(cityWeather);

        return cityWeather;
    }
}