using isun.Extensions;
using isun.Models;

namespace isun.UnitTests.Extensions;

public class WeatherModelExtensionTests
{
    [Test]
    public void ToWeatherDataEntity_WeatherModel_PropertiesCopiedCorrectly()
    {
        // Arrange
        var model = DefaultFixture.Fixture.Create<WeatherModel>();

        // Act
        var entity = model.ToWeatherDataEntity();

        // Assert
        Assert.AreEqual(model.City, entity.City);
        Assert.AreEqual(model.Precipitation, entity.Precipitation);
        Assert.AreEqual(model.Summary, entity.Summary);
        Assert.AreEqual(model.Temperature, entity.Temperature);
        Assert.AreEqual(model.WindSpeed, entity.WindSpeed);
    }
}