using isun.Models;
using isun.OutputHandlers;
using isun.Parsers;
using isun.Services;
using Microsoft.Extensions.Logging;

namespace isun.UnitTests.HostedServices;

public class WeatherPollingHostedServiceTests
{
    [Test]
    public async Task StartAsync_IncorrectCitiesArgsProvided_WeatherDataNotQueried()
    {
        // Arrange
        IList<string> _;
        var commandLineParserMock = new Mock<ICommandLineParser>();
        commandLineParserMock
            .Setup(parser => parser.TryParseArguments(It.IsAny<ICollection<string>>(), It.IsAny<string>(), It.IsAny<string>(), out _))
            .Returns(false);

        var weatherServiceMock = new Mock<IWeatherService>();
    
        var sut = new WeatherPollingHostedService(
            weatherServiceMock.Object,
            commandLineParserMock.Object,
            DefaultFixture.Fixture.Create<string[]>(),
            Mock.Of<ILogger<WeatherPollingHostedService>>(),
            Mock.Of<IOutputHandler>());
    
        // Act
        await sut.StartAsync(CancellationToken.None);
    
        // Assert
        weatherServiceMock.Verify(serv => serv.GetWeatherAsync(It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Test]
    public async Task StartAsync_GetWeatherAsyncThrows_OutputsError()
    {
        // Arrange
        var expectedException = new Exception();
        var expectedCityList = DefaultFixture.Fixture.Create<IList<string>>();
        var outHandlerMock = new Mock<IOutputHandler>();

        var commandLineParserMock = new Mock<ICommandLineParser>();
        commandLineParserMock
            .Setup(parser => parser.TryParseArguments(It.IsAny<ICollection<string>>(), It.IsAny<string>(), It.IsAny<string>(), out expectedCityList))
            .Returns(true);

        var weatherServiceMock = new Mock<IWeatherService>();
        weatherServiceMock
            .Setup(serv => serv.GetWeatherAsync(It.IsAny<IEnumerable<string>>()))
            .Throws(expectedException);

        var sut = new WeatherPollingHostedService(
            weatherServiceMock.Object,
            commandLineParserMock.Object,
            DefaultFixture.Fixture.Create<string[]>(),
            Mock.Of<ILogger<WeatherPollingHostedService>>(),
            outHandlerMock.Object);

        // Act
        sut.StartAsync(CancellationToken.None);

        // wait a bit for 1 iteration to run
        await Task.Delay(3000);
        await sut.StopAsync(CancellationToken.None);

        // Assert
        outHandlerMock.Verify(o =>
            o.WriteLine("Failed to get weather data"),
            Times.Once);
    }

    [Test]
    public async Task StartAsync_SuccessfullyGetWeatherData_OutputWeatherData()
    {
        // Arrange
        var weatherModel1 = new WeatherModel(
            DefaultFixture.Fixture.Create<string>(),
            DefaultFixture.Fixture.Create<int>(),
            DefaultFixture.Fixture.Create<int>(),
            DefaultFixture.Fixture.Create<int>(),
            DefaultFixture.Fixture.Create<string>());
        var weatherModel2 = new WeatherModel(
            DefaultFixture.Fixture.Create<string>(),
            DefaultFixture.Fixture.Create<int>(),
            DefaultFixture.Fixture.Create<int>(),
            DefaultFixture.Fixture.Create<int>(),
            DefaultFixture.Fixture.Create<string>());

        var expectedWeatherData = new List<WeatherModel> { weatherModel1, weatherModel2 };
        IList<string> expectedCityList = expectedWeatherData.Select(data => data.City).ToList();
        var outHandlerMock = new Mock<IOutputHandler>();

        var commandLineParserMock = new Mock<ICommandLineParser>();
        commandLineParserMock
            .Setup(parser => parser.TryParseArguments(It.IsAny<ICollection<string>>(), It.IsAny<string>(), It.IsAny<string>(), out expectedCityList))
            .Returns(true);

        var weatherServiceMock = new Mock<IWeatherService>();
        weatherServiceMock
            .Setup(serv => serv.GetWeatherAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(expectedWeatherData);

        var sut = new WeatherPollingHostedService(
            weatherServiceMock.Object,
            commandLineParserMock.Object,
            DefaultFixture.Fixture.Create<string[]>(),
            Mock.Of<ILogger<WeatherPollingHostedService>>(),
            outHandlerMock.Object);

        // Act
        sut.StartAsync(CancellationToken.None);

        // wait a bit for 1 iteration to run
        await Task.Delay(3000);
        await sut.StopAsync(CancellationToken.None);

        // Assert
        foreach (var weatherData in expectedWeatherData)
        {
            outHandlerMock.Verify(o =>
                o.WriteLine(It.Is<string>(a =>
                    a.Contains(weatherData.City) && a.Contains(weatherData.Temperature.ToString()))
                ),
                Times.Once);
        }
    }

    [Test]
    public async Task StartAsync_FailToGetWeatherDataForCities_OutputErrorMessage()
    {
        // Arrange
        var weatherModel1 = new WeatherModel(
            DefaultFixture.Fixture.Create<string>(),
            DefaultFixture.Fixture.Create<string>());
        var weatherModel2 = new WeatherModel(
            DefaultFixture.Fixture.Create<string>(),
            DefaultFixture.Fixture.Create<string>());

        var expectedWeatherData = new List<WeatherModel> { weatherModel1, weatherModel2 };
        IList<string> expectedCityList = expectedWeatherData.Select(data => data.City).ToList();
        var outHandlerMock = new Mock<IOutputHandler>();

        var commandLineParserMock = new Mock<ICommandLineParser>();
        commandLineParserMock
            .Setup(parser => parser.TryParseArguments(It.IsAny<ICollection<string>>(), It.IsAny<string>(), It.IsAny<string>(), out expectedCityList))
            .Returns(true);

        var weatherServiceMock = new Mock<IWeatherService>();
        weatherServiceMock
            .Setup(serv => serv.GetWeatherAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(expectedWeatherData);

        var sut = new WeatherPollingHostedService(
            weatherServiceMock.Object,
            commandLineParserMock.Object,
            DefaultFixture.Fixture.Create<string[]>(),
            Mock.Of<ILogger<WeatherPollingHostedService>>(),
            outHandlerMock.Object);

        // Act
        sut.StartAsync(CancellationToken.None);

        // wait a bit for 1 iteration to run
        await Task.Delay(3000);
        await sut.StopAsync(CancellationToken.None);

        // Assert
        foreach (var weatherData in expectedWeatherData)
        {
            outHandlerMock.Verify(o =>
                o.WriteLine(It.Is<string>(a =>
                    a.Contains(weatherData.City) && a.Contains(weatherData.Message))
                ),
                Times.Once);
        }
    }
}