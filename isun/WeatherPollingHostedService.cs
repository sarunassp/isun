using isun.OutputHandlers;
using isun.Parsers;
using isun.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace isun;

public class WeatherPollingHostedService : IHostedService
{
    private readonly IWeatherService _weatherService;
    private readonly ICommandLineParser _commandLineParser;
    private readonly string[] _citiesArgs;
    private readonly ILogger<WeatherPollingHostedService> _logger;
    private readonly IOutputHandler _outputHandler;
    private bool _isRunning = true;

    public WeatherPollingHostedService(
        IWeatherService weatherService,
        ICommandLineParser commandLineParser,
        string[] citiesArgs,
        ILogger<WeatherPollingHostedService> logger,
        IOutputHandler outputHandler)
    {
        _weatherService = weatherService;
        _commandLineParser = commandLineParser;
        _citiesArgs = citiesArgs;
        _logger = logger;
        _outputHandler = outputHandler;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_commandLineParser.TryParseArguments(_citiesArgs, "--cities", ",", out var cities) == false)
        {
            _logger.LogInformation("Cities provided in incorrect format");
            _outputHandler.WriteLine("Please provide cities in format '--cities <city1>, <city2>, <city3>, ...'");
            return;
        }

        while (_isRunning)
        {
            using(LogContext.PushProperty("CorrelationId", Guid.NewGuid()))
            {
                try
                {
                    var result = await _weatherService.GetWeatherAsync(cities);

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
                        _outputHandler.WriteLine(weatherMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get weather data");
                    _outputHandler.WriteLine("Failed to get weather data");
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