//c:\Users\Dovydas\Documents\MotoDataLogger\API\MotoDataLoggerAPI\Models\MotoData.cs
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MotoDataLoggerAPI.Models
{
    public class MotoData
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Location")]
        public int? LocationId { get; set; }
        public LocationData? Location { get; set; }
        [JsonPropertyName("Light_Sensitivity")]
        public double? LightSensitivity { get; set; }
        [JsonPropertyName("Magnetic_Field")]
        public double? MagneticField { get; set; }
        [ForeignKey("AngleData")]
        public int? AngleDataId { get; set; }
        public AngleData? AngleData { get; set; }
        [JsonPropertyName("Battery_Level")]
        public int? BatteryLevel { get; set; }
        [JsonPropertyName("Battery_Charging_Time_Left")]
        public string BatteryChargingTimeLeft { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
     [Table("LocationData")]
    public class LocationData
    {
        [Key]
        public int Id { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Altitude { get; set; }
        public double? Angle { get; set; }
        public double? Speed { get; set; }
        public double? Accuracy { get; set; }
        public double? Time { get; set; }
        public string Provider { get; set; } = string.Empty;
    }
     [Table("AngleData")]
    public class AngleData
    {
        [Key]
        public int Id { get; set; }
        public double? Azimuth { get; set; }
        public double? Pitch { get; set; }
        public double? Roll { get; set; }
    }
}

