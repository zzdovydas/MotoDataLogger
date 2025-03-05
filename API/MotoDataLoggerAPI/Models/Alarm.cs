using System;
using MotoDataLoggerAPI.Models;

namespace MotoDataLoggerAPI.Models
{
    public enum AlarmState
    {
        Unlocked,
        Locked,
        Disabled
    }

    public class Alarm
    {
        // Alarm Configuration
        public AlarmState State { get; set; } = AlarmState.Disabled;
        public int DataPullIntervalSeconds { get; set; } = 60; // Default: 60 seconds
        public int MovementSensitivity { get; set; } = 5; // Meters. How much the motorcycle must move to trigger the  

        // Last Known State
        public DateTime LastDataReceived { get; set; } = DateTime.MinValue;
        public LocationData? LastKnownLocation { get; set; }
        public double? LastKnownLightSensitivity { get; set; }
        public double? LastKnownMagneticField { get; set; }

        // Alarm Triggered
        public bool IsAlarmTriggered { get; private set; } = false;
        public string? AlarmTriggerReason { get; private set; } = string.Empty;

        // Methods to Check and Update Alarm State
        public void CheckForAlarmTrigger(MotoData currentData)
        {
            if (State == AlarmState.Disabled)
            {
                IsAlarmTriggered = false;
                AlarmTriggerReason = string.Empty;
                 UpdateLastKnownState(currentData);
                return;
            }
            if (currentData == null)
            {
                IsAlarmTriggered = false;
                AlarmTriggerReason = "No data received";
                 UpdateLastKnownState(currentData);
                return;
            }
            IsAlarmTriggered = false;
            AlarmTriggerReason = string.Empty;

            if (State == AlarmState.Locked)
            {
                if (LastKnownLocation != null && currentData.Location != null)
                {
                    var distanceMoved = CalculateDistance(LastKnownLocation, currentData.Location);

                    if (distanceMoved > MovementSensitivity)
                    {
                        IsAlarmTriggered = true;
                        AlarmTriggerReason += $"Moved {distanceMoved} meters from the last known location. ";
                    }
                }
                if (LastKnownLightSensitivity != null && currentData.LightSensitivity != null)
                {
                    if (Math.Abs(LastKnownLightSensitivity.Value - currentData.LightSensitivity.Value) > 2.0)
                    {
                        IsAlarmTriggered = true;
                        AlarmTriggerReason += $"Light sensitivity changed significantly. ";
                    }
                }
                if (LastKnownMagneticField != null && currentData.MagneticField != null)
                {
                   if (Math.Abs(LastKnownMagneticField.Value - currentData.MagneticField.Value) > 10.0)
                    {
                        IsAlarmTriggered = true;
                        AlarmTriggerReason += $"Magnetic field changed significantly. ";
                    }
                }
            }

            // Update Last Known State
             UpdateLastKnownState(currentData);
        }

        private void UpdateLastKnownState(MotoData currentData)
        {
             if(currentData != null)
            {
                 LastDataReceived = currentData.Timestamp;
                if (currentData.Location != null)
                {
                    LastKnownLocation = new LocationData()
                    {
                        Latitude = currentData.Location.Latitude,
                        Longitude = currentData.Location.Longitude,
                        Altitude = currentData.Location.Altitude,
                        Angle = currentData.Location.Angle,
                        Speed = currentData.Location.Speed,
                        Accuracy = currentData.Location.Accuracy,
                        Time = currentData.Location.Time,
                        Provider = currentData.Location.Provider
                    };
                }
                if (currentData.LightSensitivity.HasValue)
                {
                     LastKnownLightSensitivity = currentData.LightSensitivity.Value;
                }
                if (currentData.MagneticField.HasValue)
                {
                     LastKnownMagneticField = currentData.MagneticField.Value;
                }
            }
        }

        // Helper Method to Calculate Distance
        private double CalculateDistance(LocationData loc1, LocationData loc2)
        {
            // Very simplified distance calculation (assuming earth is flat)
            if (loc1.Latitude == null || loc1.Longitude == null || loc2.Latitude == null || loc2.Longitude == null)
                return 0;

            var latDiff = loc2.Latitude - loc1.Latitude;
            var lonDiff = loc2.Longitude - loc1.Longitude;
            return Math.Sqrt(latDiff.Value * latDiff.Value + lonDiff.Value * lonDiff.Value) * 111139; // Convert to meters
        }
    }
}
