using isun.Entities.Http;
using isun.Extensions;

namespace isun.UnitTests.Extensions;

public class WeatherEntityExtensionTests
{
    [Test]
    public void ToWeatherModel_WeatherEntity_PropertiesCopiedCorrectly()
    {
        // Arrange
        var entity = DefaultFixture.Fixture.Create<WeatherEntity>();

        // Act
        var model = entity.ToWeatherModel();

        // Assert
        Assert.AreEqual(entity.City, model.City);
        Assert.AreEqual(entity.Precipitation, model.Precipitation);
        Assert.AreEqual(entity.Summary, model.Summary);
        Assert.AreEqual(entity.Temperature, model.Temperature);
        Assert.AreEqual(entity.WindSpeed, model.WindSpeed);
    }
}