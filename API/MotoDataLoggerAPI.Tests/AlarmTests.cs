using MotoDataLoggerAPI.Models;
using System;
using Xunit;
using FluentAssertions;

namespace MotoDataLoggerAPI.Tests.Models
{
    public class AlarmTests
    {
        [Fact]
        public void Alarm_DefaultConstructor_SetsPropertiesToDefaultValues()
        {
            // Arrange
            var alarm = new Alarm();

            // Assert
            alarm.State.Should().Be(AlarmState.Disabled);
            alarm.DataPullIntervalSeconds.Should().Be(60);
            alarm.MovementSensitivity.Should().Be(5);
            alarm.LastDataReceived.Should().Be(DateTime.MinValue);
            alarm.LastKnownLocation.Should().BeNull();
            alarm.LastKnownLightSensitivity.Should().BeNull();
            alarm.LastKnownMagneticField.Should().BeNull();
            alarm.IsAlarmTriggered.Should().BeFalse();
            alarm.AlarmTriggerReason.Should().BeEmpty();
        }

        [Fact]
        public void CheckForAlarmTrigger_DisabledState_DoesNotTriggerAlarm()
        {
            // Arrange
            var alarm = new Alarm { State = AlarmState.Disabled };
            var motoData = new MotoData { Timestamp = DateTime.Now };

            // Act
            alarm.CheckForAlarmTrigger(motoData);

            // Assert
            alarm.IsAlarmTriggered.Should().BeFalse();
            alarm.AlarmTriggerReason.Should().BeEmpty();
        }

        [Fact]
        public void CheckForAlarmTrigger_NullData_DoesNotTriggerAlarm()
        {
            // Arrange
            var alarm = new Alarm { State = AlarmState.Locked };

            // Act
            alarm.CheckForAlarmTrigger(null);

            // Assert
            alarm.IsAlarmTriggered.Should().BeFalse();
            alarm.AlarmTriggerReason.Should().Be("No data received");
        }

        [Fact]
        public void CheckForAlarmTrigger_LockedState_MovementTriggersAlarm()
        {
            // Arrange
            var alarm = new Alarm { State = AlarmState.Locked, MovementSensitivity = 10 };
            alarm.LastKnownLocation = new LocationData { Latitude = 50.0, Longitude = 15.0 };
            var currentData = new MotoData
            {
                Timestamp = DateTime.Now,
                Location = new LocationData { Latitude = 50.0001, Longitude = 15.0 } // Moved approximately 11 meters (more than 10)
            };

            // Act
            alarm.CheckForAlarmTrigger(currentData);

            // Assert
            alarm.IsAlarmTriggered.Should().BeTrue();
            alarm.AlarmTriggerReason.Should().Contain("Moved");
        }

        [Fact]
        public void CheckForAlarmTrigger_LockedState_NoMovementDoesNotTriggerAlarm()
        {
            // Arrange
            var alarm = new Alarm { State = AlarmState.Locked, MovementSensitivity = 10 };
            alarm.LastKnownLocation = new LocationData { Latitude = 50.0, Longitude = 15.0 };
            var currentData = new MotoData
            {
                Timestamp = DateTime.Now,
                Location = new LocationData { Latitude = 50.0, Longitude = 15.0 } // No movement
            };

            // Act
            alarm.CheckForAlarmTrigger(currentData);

            // Assert
            alarm.IsAlarmTriggered.Should().BeFalse();
            alarm.AlarmTriggerReason.Should().BeEmpty();
        }
        [Fact]
        public void CheckForAlarmTrigger_LockedState_LightSensitivityTriggersAlarm()
        {
            // Arrange
            var alarm = new Alarm { State = AlarmState.Locked, MovementSensitivity = 10 };
            alarm.LastKnownLightSensitivity = 5.0;
            var currentData = new MotoData
            {
                Timestamp = DateTime.Now,
                LightSensitivity = 8.0
            };

            // Act
            alarm.CheckForAlarmTrigger(currentData);

            // Assert
            alarm.IsAlarmTriggered.Should().BeTrue();
            alarm.AlarmTriggerReason.Should().Contain("Light sensitivity");
        }
        [Fact]
        public void CheckForAlarmTrigger_LockedState_MagneticFieldTriggersAlarm()
        {
            // Arrange
            var alarm = new Alarm { State = AlarmState.Locked, MovementSensitivity = 10 };
            alarm.LastKnownMagneticField = 40.0;
            var currentData = new MotoData
            {
                Timestamp = DateTime.Now,
                MagneticField = 60.0
            };

            // Act
            alarm.CheckForAlarmTrigger(currentData);

            // Assert
            alarm.IsAlarmTriggered.Should().BeTrue();
            alarm.AlarmTriggerReason.Should().Contain("Magnetic field");
        }
        [Fact]
        public void UpdateLastKnownState_UpdatesPropertiesCorrectly()
        {
            // Arrange
            var alarm = new Alarm();
            var motoData = new MotoData
            {
                Timestamp = new DateTime(2024, 01, 01),
                Location = new LocationData { Latitude = 51.0, Longitude = 16.0, Altitude = 200, Angle = 90, Speed = 10, Accuracy = 2, Time = 1, Provider = "gps" },
                LightSensitivity = 6.5,
                MagneticField = 30.0
            };

            // Act
            alarm.CheckForAlarmTrigger(motoData);

            // Assert
            alarm.LastDataReceived.Should().Be(new DateTime(2024, 01, 01));
            alarm.LastKnownLocation.Should().NotBeNull();
            alarm.LastKnownLocation.Latitude.Should().Be(51.0);
            alarm.LastKnownLocation.Longitude.Should().Be(16.0);
            alarm.LastKnownLocation.Altitude.Should().Be(200);
            alarm.LastKnownLocation.Angle.Should().Be(90);
            alarm.LastKnownLocation.Speed.Should().Be(10);
            alarm.LastKnownLocation.Accuracy.Should().Be(2);
            alarm.LastKnownLocation.Time.Should().Be(1);
            alarm.LastKnownLocation.Provider.Should().Be("gps");
            alarm.LastKnownLightSensitivity.Should().Be(6.5);
            alarm.LastKnownMagneticField.Should().Be(30.0);
        }
         [Fact]
        public void CheckForAlarmTrigger_LockedState_NullLocation_DoesNotCrash()
        {
            // Arrange
            var alarm = new Alarm { State = AlarmState.Locked, MovementSensitivity = 10 };
            alarm.LastKnownLocation = new LocationData { Latitude = 50.0, Longitude = 15.0 };
            var currentData = new MotoData
            {
                Timestamp = DateTime.Now,
                Location = null
            };

            // Act
            alarm.CheckForAlarmTrigger(currentData);

            // Assert
             alarm.IsAlarmTriggered.Should().BeFalse();
            alarm.AlarmTriggerReason.Should().BeEmpty();
        }
          [Fact]
        public void CheckForAlarmTrigger_LockedState_NullLastLocation_DoesNotCrash()
        {
            // Arrange
            var alarm = new Alarm { State = AlarmState.Locked, MovementSensitivity = 10 };
            alarm.LastKnownLocation = null;
            var currentData = new MotoData
            {
                Timestamp = DateTime.Now,
                Location = new LocationData { Latitude = 50.0001, Longitude = 15.0 }
            };

            // Act
            alarm.CheckForAlarmTrigger(currentData);

            // Assert
             alarm.IsAlarmTriggered.Should().BeFalse();
            alarm.AlarmTriggerReason.Should().BeEmpty();
        }
    }
}
