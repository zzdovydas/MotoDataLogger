using MotoDataLoggerAPI.Models;
using Xunit;
using FluentAssertions;

namespace MotoDataLoggerAPI.Tests.Models
{
    public class AngleDataTests
    {
        [Fact]
        public void AngleData_DefaultConstructor_SetsPropertiesToDefaultValues()
        {
            // Arrange
            var angleData = new AngleData();

            // Assert
            angleData.Azimuth.Should().BeNull();
            angleData.Pitch.Should().BeNull();
            angleData.Roll.Should().BeNull();
        }

        [Fact]
        public void AngleData_SetAndGetProperties_ReturnsCorrectValues()
        {
            // Arrange
            var angleData = new AngleData
            {
                Azimuth = 180.0,
                Pitch = -10.5,
                Roll = 5.2
            };

            // Assert
            angleData.Azimuth.Should().Be(180.0);
            angleData.Pitch.Should().Be(-10.5);
            angleData.Roll.Should().Be(5.2);
        }
    }
}
