using System.Text.Json.Serialization;

namespace MotoDataLoggerAPI.Models
{
    public class MotoData
    {
        public LocationData? Location { get; set; } = new LocationData();
        [JsonPropertyName("Light_Sensitivity")]
        public double? LightSensitivity { get; set; }
        [JsonPropertyName("Magnetic_Field")]
        public double? MagneticField { get; set; }
        [JsonPropertyName("Angle_Data")]
        public AngleData? AngleData { get; set; } = new AngleData();
        [JsonPropertyName("Battery_Level")]
        public int? BatteryLevel { get; set; }
        [JsonPropertyName("Battery_Charging_Time_Left")]
        public string BatteryChargingTimeLeft { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class LocationData
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Altitude { get; set; }
        public double? Angle { get; set; }
        public double? Speed { get; set; }
        public double? Accuracy { get; set; }
        public double? Time { get; set; }
        public string Provider { get; set; } = string.Empty;
    }

    public class AngleData
    {
        public double? Azimuth { get; set; }
        public double? Pitch { get; set; }
        public double? Roll { get; set; }
    }
}
