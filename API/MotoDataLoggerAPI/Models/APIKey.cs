using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MotoDataLoggerAPI.Models
{
    public class ApiKey
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Key { get; set; } = string.Empty;

        [ForeignKey("Motorcycle")]
        public int MotorcycleId { get; set; }
        [JsonIgnore]
        public Motorcycle? Motorcycle { get; set; }
    }
}
