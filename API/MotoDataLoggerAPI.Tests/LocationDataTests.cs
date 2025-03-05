using MotoDataLoggerAPI.Models;
using Xunit;
using FluentAssertions;

namespace MotoDataLoggerAPI.Tests.Models
{
    public class LocationDataTests
    {
        [Fact]
        public void LocationData_DefaultConstructor_SetsPropertiesToDefaultValues()
        {
            // Arrange
            var locationData = new LocationData();

            // Assert
            locationData.Latitude.Should().BeNull();
            locationData.Longitude.Should().BeNull();
            locationData.Altitude.Should().BeNull();
            locationData.Angle.Should().BeNull();
            locationData.Speed.Should().BeNull();
            locationData.Accuracy.Should().BeNull();
            locationData.Time.Should().BeNull();
            locationData.Provider.Should().BeEmpty();
        }

        [Fact]
        public void LocationData_SetAndGetProperties_ReturnsCorrectValues()
        {
            // Arrange
            var locationData = new LocationData
            {
                Latitude = 50.123,
                Longitude = 14.567,
                Altitude = 150.5,
                Angle = 90.0,
                Speed = 120.5,
                Accuracy = 5.0,
                Time = 1234567890,
                Provider = "network"
            };

            // Assert
            locationData.Latitude.Should().Be(50.123);
            locationData.Longitude.Should().Be(14.567);
            locationData.Altitude.Should().Be(150.5);
            locationData.Angle.Should().Be(90.0);
            locationData.Speed.Should().Be(120.5);
            locationData.Accuracy.Should().Be(5.0);
            locationData.Time.Should().Be(1234567890);
            locationData.Provider.Should().Be("network");
        }
    }
}
