using CommandLine;
using isun.Configuration;
using isun.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace isun;

public class WeatherPollingHostedService : IHostedService
{
    private readonly IWeatherService _weatherService;
    private readonly string[] _citiesArgs;
    private readonly ILogger<WeatherPollingHostedService> _logger;
    private bool _isRunning = true;

    public WeatherPollingHostedService(IWeatherService weatherService, string[] citiesArgs, ILogger<WeatherPollingHostedService> logger)
    {
        _weatherService = weatherService;
        _citiesArgs = citiesArgs;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_citiesArgs == null)
        {
            _logger.LogInformation("'--cities' not provided in arguments");
            return;
        }

        var parser = new Parser();
        var parsedArgs = parser.ParseArguments<CommandLineOptions>(_citiesArgs);

        if (parsedArgs.Value?.Cities.Any() != true)
        {
            _logger.LogInformation("No valid '--cities' provided");
            return;
        }

        while (_isRunning)
        {
            using(LogContext.PushProperty("CorrelationId", Guid.NewGuid()))
            {
                try
                {
                    var result = await _weatherService.GetWeatherAsync(parsedArgs.Value.Cities);

                    foreach (var weatherModel in result)
                    {
                        string weatherMessage;
                        if (weatherModel.Message != null)
                        {
                            weatherMessage = $"{weatherModel.City} - {weatherModel.Message}";
                        }
                        else
                        {
                            weatherMessage =
                                $"{weatherModel.City} - " +
                                $"{nameof(weatherModel.Temperature)}:{weatherModel.Temperature}, " +
                                $"{nameof(weatherModel.Precipitation)}:{weatherModel.Precipitation}, " +
                                $"{nameof(weatherModel.WindSpeed)}:{weatherModel.WindSpeed}, " +
                                $"{nameof(weatherModel.Summary)}:{weatherModel.Summary}";
                        }
                        
                        _logger.LogInformation(weatherMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get weather data");
                }
                finally
                {
                    await Task.Delay(15000, cancellationToken);
                }
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _isRunning = false;
        return Task.CompletedTask;
    }
}