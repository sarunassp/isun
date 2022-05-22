using System.Reflection;
using isun.Configuration;
using isun.Http.Clients;
using isun.Http.Handlers;
using isun.Repositories;
using isun.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace isun
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("isun.appsettings.json");
                    config.AddJsonStream(resourceStream);
                })
                .UseSerilog((context, config) =>
                {
                    config
                        .ReadFrom.Configuration(context.Configuration)
                        .Enrich.FromLogContext()
                        .WriteTo.Console();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<WeatherClientSettings>(options =>
                        context.Configuration
                            .GetSection(WeatherClientSettings.Section)
                            .Get<WeatherClientSettings>(binderOptions => binderOptions.BindNonPublicProperties = true)
                        );

                    services.AddTransient<LoggingHandler>();
                    services
                        .AddHttpClient(Constants.HttpClientNames.BasicWeatherClient, (provider, httpClient) =>
                        {
                            var weatherSettings = provider.GetService<WeatherClientSettings>();
                            httpClient.BaseAddress = weatherSettings.BaseUri;
                        })
                        .AddHttpMessageHandler<LoggingHandler>();
                    
                    services.AddTransient<AuthenticationHandler>();
                    services
                        .AddHttpClient(Constants.HttpClientNames.AuthenticatedWeatherClient, (provider, httpClient) =>
                        {
                            var weatherSettings = provider.GetService<WeatherClientSettings>();
                            httpClient.BaseAddress = weatherSettings.BaseUri;
                        })
                        .AddHttpMessageHandler<LoggingHandler>()
                        .AddHttpMessageHandler<AuthenticationHandler>();


                    services.AddSingleton<IWeatherClient, WeatherClient>();
                    services.AddSingleton<IWeatherRepository, WeatherRepository>();

                    services.AddSingleton<IWeatherService, WeatherService>();

                    services.AddHostedService<WeatherPollingHostedService>(provider =>
                    {
                        var service = provider.GetService<IWeatherService>();
                        var logger = provider.GetService<ILogger<WeatherPollingHostedService>>();
                        return new WeatherPollingHostedService(service, args, logger);
                    });
                })
                .Build();

            await host.RunAsync();
        }
    }
}