using isun.Models;
using isun.Services;
using Microsoft.Extensions.Logging;

namespace isun.UnitTests.HostedServices;

public class WeatherPollingHostedServiceTests
{
    [Theory]
    [TestCaseSource(nameof(CitiesArgsCases))]
    public async Task StartAsync_IncorrectCitiesArgsProvided_WeatherDataNotQueried(string[] citiesArgs)
    {
        // Arrange
        var weatherServiceMock = new Mock<IWeatherService>();

        var sut = new WeatherPollingHostedService(
            weatherServiceMock.Object,
            citiesArgs,
            Mock.Of<ILogger<WeatherPollingHostedService>>());

        // Act
        await sut.StartAsync(CancellationToken.None);

        // Assert
        weatherServiceMock.Verify(serv => serv.GetWeatherAsync(It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Test]
    public async Task StartAsync_GetWeatherAsyncThrows_LogsError()
    {
        // Arrange
        var expectedException = new Exception();

        var loggerMock = new Mock<ILogger<WeatherPollingHostedService>>();

        var weatherServiceMock = new Mock<IWeatherService>();
        weatherServiceMock
            .Setup(serv => serv.GetWeatherAsync(It.IsAny<IEnumerable<string>>()))
            .Throws(expectedException);

        var sut = new WeatherPollingHostedService(
            weatherServiceMock.Object,
            new[] { "--cities", "Vilnius" },
            loggerMock.Object);

        // Act
        sut.StartAsync(CancellationToken.None);

        // wait a bit for 1 iteration to run
        await Task.Delay(3000);
        await sut.StopAsync(CancellationToken.None);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task StartAsync_GetWeatherAsyncThrows_LogsWeatherData()
    {
        // Arrange
        var expectedWeatherData = DefaultFixture.Fixture.Create<IEnumerable<WeatherModel>>();
        var loggerMock = new Mock<ILogger<WeatherPollingHostedService>>();

        var weatherServiceMock = new Mock<IWeatherService>();
        weatherServiceMock
            .Setup(serv => serv.GetWeatherAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(expectedWeatherData);

        var sut = new WeatherPollingHostedService(
            weatherServiceMock.Object,
            new[] { "--cities", "Vilnius" },
            loggerMock.Object);

        // Act
        sut.StartAsync(CancellationToken.None);

        // wait a bit for 1 iteration to run
        await Task.Delay(3000);
        await sut.StopAsync(CancellationToken.None);

        // Assert
        foreach (var weatherData in expectedWeatherData)
        {
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains(weatherData.City) &&
                                                  o.ToString().Contains(weatherData.Temperature.ToString())),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    private static IEnumerable<string[]> CitiesArgsCases
    {
        get
        {
            yield return new string[] { "--notCities", "some", "random", "strings" };
            yield return new string[] { "" };
            yield return new string[] { };
            yield return (string[])null;
        }
    }
}