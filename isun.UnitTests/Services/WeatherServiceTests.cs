using isun.Entities.Http;
using isun.Http.Clients;
using isun.Models;
using isun.Repositories;
using isun.Services;
using Microsoft.Extensions.Logging;

namespace isun.UnitTests.Services;

public class WeatherServiceTests
{
    [Test]
    public async Task GetWeatherAsync_RequestedCityNotFound_DoesNotHaveWeatherInformation()
    {
        // Arrange
        var weatherClientMock = new Mock<IWeatherClient>();
        weatherClientMock
            .Setup(client => client.GetCitiesAsync())
            .ReturnsAsync(new[] { "City1", "City2"});

        var sut = new WeatherService(
            weatherClientMock.Object,
            Mock.Of<IWeatherRepository>(),
            Mock.Of<ILogger<WeatherService>>());

        var cityName = DefaultFixture.Fixture.Create<string>();

        // Act
        var result = await sut.GetWeatherAsync(new[] { cityName });

        // Assert
        Assert.False(result.Single().HasWeatherInformation);
        Assert.AreEqual("Weather data not available for city", result.Single().Message);
    }

    [Test]
    public async Task GetWeatherAsync_FailToGetWeatherInformation_DoesNotThrow()
    {
        // Arrange
        var cityName = DefaultFixture.Fixture.Create<string>();

        var weatherClientMock = new Mock<IWeatherClient>();
        weatherClientMock
            .Setup(client => client.GetCitiesAsync())
            .ReturnsAsync(new[] { "City1", "City2", cityName });
        weatherClientMock
            .Setup(client => client.GetWeatherAsync(It.IsAny<string>()))
            .Throws<Exception>();

        var sut = new WeatherService(
            weatherClientMock.Object,
            Mock.Of<IWeatherRepository>(),
            Mock.Of<ILogger<WeatherService>>());

        // Act
        var result = await sut.GetWeatherAsync(new[] { cityName });

        // Assert
        Assert.False(result.Single().HasWeatherInformation);
        Assert.AreEqual("Failed to get weather data for city", result.Single().Message);
    }

    [Test]
    public async Task GetWeatherAsync_WeatherDataFound_WeatherDataSaved()
    {
        // Arrange
        var cityName = DefaultFixture.Fixture.Create<string>();
        var expectedWeatherEntity = DefaultFixture.Fixture.Create<WeatherEntity>();

        var weatherClientMock = new Mock<IWeatherClient>();
        weatherClientMock
            .Setup(client => client.GetCitiesAsync())
            .ReturnsAsync(new[] { "City1", "City2", cityName });
        weatherClientMock
            .Setup(client => client.GetWeatherAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedWeatherEntity);

        var weatherRepositoryMock = new Mock<IWeatherRepository>();

        var sut = new WeatherService(
            weatherClientMock.Object,
            weatherRepositoryMock.Object,
            Mock.Of<ILogger<WeatherService>>());

        // Act
        var result = await sut.GetWeatherAsync(new[] { cityName });

        // Assert
        var cityWeather = result.Single();
        Assert.AreEqual(expectedWeatherEntity.City, cityWeather.City);
        Assert.AreEqual(expectedWeatherEntity.Precipitation, cityWeather.Precipitation);
        Assert.AreEqual(expectedWeatherEntity.Summary, cityWeather.Summary);
        Assert.AreEqual(expectedWeatherEntity.Temperature, cityWeather.Temperature);
        Assert.AreEqual(expectedWeatherEntity.WindSpeed, cityWeather.WindSpeed);

        weatherRepositoryMock.Verify(
            repo => repo.SaveWeatherData(It.IsAny<IReadOnlyCollection<WeatherModel>>()),
            Times.Once);
    }
}